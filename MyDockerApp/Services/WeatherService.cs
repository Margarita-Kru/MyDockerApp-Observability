using Microsoft.Extensions.Caching.Distributed;
using MyDockerApp.Data;
using MyDockerApp.Dtos;
using MyDockerApp.Entities;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MyDockerApp.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly ILogger<WeatherService> _logger;

        private readonly AppDbContext _appDbContext;

        private readonly IDistributedCache _distributedCache;
        private readonly IKafkaProducerService _kafkaProducer;

        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);

        private static int _totalForecastGenerated = 0;
        private static readonly ConcurrentDictionary<int, string> _weatherCache = new();
        private static readonly string[] summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"

        };

        public WeatherService(ILogger<WeatherService> logger,
        AppDbContext dbContext,
        IDistributedCache distributedCache,
        IKafkaProducerService producer)
        {
            _logger = logger;
            _appDbContext = dbContext;
            _distributedCache = distributedCache;
            _kafkaProducer = producer;
        }

        public async Task<IEnumerable<WeatherForecast>> GenerateAndSaveWeatherAsync(int maxItems)
        {
            //_logger.Log(LogLevel.Information, "Пользователь {UserId} запросил погоду. Город: {City}", 42, "Moscow");

            //_logger.LogInformation("Используется ключ {Key}", _options.Value.ApiKey);

            var newRecords = Enumerable.Range(1, maxItems).Select(index =>
            new WeatherRecord
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
            .ToList();

            _appDbContext.AddRange(newRecords);

            await _appDbContext.SaveChangesAsync();

            int currentCount = Interlocked.Increment(ref _totalForecastGenerated);

            _logger.LogInformation("Всего прогнозов сгенерировано: {Count}", currentCount);

            await _kafkaProducer.SendMessageAsync("weather-events", $"Создана запись о погоде: {newRecords[0].Id}");

            return newRecords.Select(r => new WeatherForecast(r.Date, r.TemperatureC, r.Summary));
        }

        public async Task<WeatherRecord?> GetByIdAsync(int id)
        {
            string cachekey = $"weather_record_{id}";
            var cacheBytes = await _distributedCache.GetAsync(cachekey);
            if (cacheBytes != null)
            {
                //десериализация
                var cachedRecord = JsonSerializer.Deserialize<WeatherRecord>(cacheBytes);
                _logger.LogInformation("Данные взяты из кэша!");
                return cachedRecord;
            }
            var record = await _appDbContext.WeatherRecords.FindAsync(id);
            if (record == null) return null;

            var serializedRecord = JsonSerializer.SerializeToUtf8Bytes(record);
            var cachedOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _distributedCache.SetAsync(cachekey, serializedRecord, cachedOptions);
            _logger.LogInformation("Данные взяты из БД и сохранены в кэш.");
            return record;
        }

        public Task<string> GetWeatherCachedAsync(int id)
        {
            if(_weatherCache.TryGetValue(id, out var cachedValue))
            {
                return Task.FromResult(cachedValue);
            }
            var newValue = "$Погода для {id}";
            _weatherCache.GetOrAdd(id, newValue);
            return Task.FromResult(newValue);
        }

        public async Task<WeatherRecord?> UpdateAsync(int id, WeatherForecast updatedForecast)
        {
            var record = await _appDbContext.WeatherRecords.FindAsync(id);
            if (record == null) return null;

            record.TemperatureC = updatedForecast.TemperatureC;
            record.Summary = updatedForecast.Summary;

            string cachedkey = $"weather_record_{id}";

            await _appDbContext.SaveChangesAsync();

            await _distributedCache.RemoveAsync(cachedkey); // инвалидация кэша
            return record;
        }

        public async Task<WeatherRecord?> DeleteAsync(int id)
        {
            var record = await _appDbContext.WeatherRecords.FindAsync(id);
            if (record == null) return null;

            string cachedkey = $"weather_record_{id}";

            _appDbContext.WeatherRecords.Remove(record);
            await _appDbContext.SaveChangesAsync();

            await _distributedCache.RemoveAsync(cachedkey); // инвалидация кэша
            return record;
        }

        public async Task<string> CallSlowExternalApiAsync()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                _logger.LogInformation("Зашли в критическую секцию. Имитируем долгий запрос...");
                await Task.Delay(2000); // Имитация долгого HTTP-запроса
                return "Данные от API";
            }
            finally { semaphoreSlim.Release(); }
        }
    }
}
