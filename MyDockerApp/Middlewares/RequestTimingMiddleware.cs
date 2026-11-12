using System.Diagnostics;

namespace MyDockerApp.Middlewares;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;
    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally {
            sw.Stop();
            string method = context.Request.Method;
            string path = context.Request.Path;
            long time = sw.ElapsedMilliseconds;

            _logger.Log(
                LogLevel.Information,
                "HTTP {Method} {Path} выполнен за {time} мс",
                method,
                path,
                time);
        }
    }
}
