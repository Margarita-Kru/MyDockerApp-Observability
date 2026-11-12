using MyDockerApp.Interfaces;

namespace MyDockerApp.Services;

public class CorrelationService : ICorrelationService
{
    public string CorrelationId { get; set; } = string.Empty;
}
