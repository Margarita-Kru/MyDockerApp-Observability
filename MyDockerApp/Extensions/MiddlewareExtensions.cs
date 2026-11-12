using MyDockerApp.Interfaces;
using MyDockerApp.Middlewares;
using MyDockerApp.Services;

namespace MyDockerApp.Extensions;

public static class MiddlewareExtensions
{
    public static IServiceCollection AddRequestCorrelationServices(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationService, CorrelationService>();
        services.AddTransient<RequestCorrelationMiddleware>();
        return services;
    }
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestTimingMiddleware>();
    }

    public static IApplicationBuilder UseRequestCorrelation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestCorrelationMiddleware>();
    }

    public static IApplicationBuilder UseIpFilter(this IApplicationBuilder app, string blockedIp)
    {
        return app.UseMiddleware<IpFilterMiddleware>(blockedIp);
    }
}
