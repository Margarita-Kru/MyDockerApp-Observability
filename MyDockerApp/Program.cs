using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyDockerApp.Clients;
using MyDockerApp.Data;
using MyDockerApp.Extensions;
using MyDockerApp.Interfaces;
using MyDockerApp.OptionsClasses;
using MyDockerApp.Services;
using Polly;
using Prometheus;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.Graylog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Graylog(new GraylogSinkOptions
    {
        HostnameOrAddress = "graylog",
        Port = 12201,
        TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp
    })
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddProblemDetails(); // сервис для форматирования ошибки с красивым JSON

//builder.Services = коллекция, куда записываются правила, актуальна на момент запуска приложения
//app.Services = собранный и готовый контейнер, используется во время работы приложения (рантайм)
builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddRequestCorrelationServices();

builder.Services.AddTransient<ITransientService, TransientService>();
builder.Services.AddSingleton<ISingletonService, SingletonService>();
builder.Services.AddScoped<IScopedService, ScopedService>();

builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.AddOptions<WeatherApiOptions>()
    .Bind(builder.Configuration.GetSection("WeatherApi"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<RedisOptions>()
    .Bind(builder.Configuration.GetSection("Redis"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>();
if (redisOptions == null) throw new Exception("Секция Redis не найдена в конфигурации");
var redisConnectionString = redisOptions.ConnectionString;

builder.Services.AddHealthChecks().
    AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgres")
    .AddRedis(redisConnectionString, name: "redis");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisOptions.ConnectionString;
    options.InstanceName = redisOptions.InstanceName;
});

var retryPolicy = Policy
    .Handle<RedisConnectionException>()
    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

IConnectionMultiplexer redis = null!;
try
{
    redis = retryPolicy.Execute(() => ConnectionMultiplexer.Connect(redisConnectionString));
}
catch (Exception ex)
{
    // Логируем и падаем — Fail-Fast
    Log.Fatal(ex, "Не удалось подключиться к Redis при старте");
    throw;
}
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        )
    ));

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddTokenBucketLimiter("token", options =>
    {
        options.TokenLimit = 5;
        options.TokensPerPeriod = 1;
        options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        options.AutoReplenishment = true;
        options.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("Слишком много запросов. Подождите немного и попробуйте снова.", token);
    };
});

// настройка Named Client
builder.Services.AddHttpClient("DogClient", client =>
{
    client.BaseAddress = new Uri("https://dog.ceo/api/breeds/image/");
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.Add("User-Agent", "MyDockerApp");
});

builder.Services.AddHttpClient("JsonPlaceholderClient", client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});

//настройка Typed Client
builder.Services.AddHttpClient<ReqResClient>(client =>
{
    client.BaseAddress = new Uri("https://reqres.in/api/");
    client.DefaultRequestHeaders.Add("x-api-key", "reqres-free-v1");
});

var app = builder.Build();

app.UseExceptionHandler(); // для необработанных исключений (ошибка в бд или деление на 0)
app.UseStatusCodePages(); // для перехвата пустых ответов и заполнения стандартным JSON от ProblemDetails

app.UseRequestTiming();
app.UseRequestCorrelation();

app.UseHttpMetrics(); // Собирает метрики по HTTP-запросам (время ответа, коды и т.д.)
app.MapMetrics();     // Открывает эндпоинт /metrics


app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/integrations"),
    appBuilder =>
    {
        appBuilder.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Admin-Path", "true");
            await next();
        });
    });
app.MapWhen(context => context.Request.Path == "/maintenance",
    appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("Сайт на техническом обслуживании. Зайдите позже.");
        });
    });

app.UseStaticFiles(); // до ограничения частоты запросов

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapControllers();

using (var scope = app.Services.CreateScope()) // только в учебном проекте, в продакшн в CI/CD
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Применяем миграции при старте
}

app.Run();

