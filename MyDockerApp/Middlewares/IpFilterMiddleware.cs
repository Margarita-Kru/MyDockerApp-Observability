namespace MyDockerApp.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class IpFilterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpFilterMiddleware> _logger;
        private readonly string _blockedIp;

        public IpFilterMiddleware(RequestDelegate next, ILogger<IpFilterMiddleware> logger, string ip)
        {
            _next = next;
            _logger = logger;
            _blockedIp = ip;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var ip = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            _logger.LogInformation("Входящий запрос с IP: {IP}", ip);

            if (ip == _blockedIp)
            {
                _logger.LogWarning("Заблокирован доступ для IP: {ip}", ip);
                httpContext.Response.StatusCode = 403;
                httpContext.Response.ContentType = "text/plain; charset=utf-8";
                await httpContext.Response.WriteAsync("Доступ запрещен.");
                return; //терминальный метод
            }
            await _next(httpContext);
        }
    }
}
