using MyDockerApp.Interfaces;
using Serilog.Context;

namespace MyDockerApp.Middlewares
{
    public class RequestCorrelationMiddleware : IMiddleware
    {
        private readonly ILogger<RequestCorrelationMiddleware> _logger;
        private readonly ICorrelationService _correlationService;
        private const string CorrelationHeader = "X-Correlation-Id";

        public RequestCorrelationMiddleware(ICorrelationService correlationService, ILogger<RequestCorrelationMiddleware> logger)
        {
            _correlationService = correlationService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string correlationId;
            if (context.Request.Headers.TryGetValue(CorrelationHeader, out var headerValue))
            {
                correlationId = headerValue.ToString();
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // Сохраняем в наш Scoped-сервис (чтобы другие сервисы в этом запросе могли его достать)
            _correlationService.CorrelationId = correlationId;

            // Добавляем ID в заголовок ответа
            context.Response.Headers.Append(CorrelationHeader, correlationId);

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    await next(context);
                }
                finally
                {
                    _logger.LogInformation("HTTP Correlation ID {Id} {Path}", correlationId, context.Request.Path);
                }
            }
        }
    }
}
