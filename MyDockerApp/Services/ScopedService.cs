using MyDockerApp.Interfaces;

namespace MyDockerApp.Services;

public class ScopedService : IScopedService
{
    private string TestGuid { get; set; } = string.Empty;

    public ScopedService()
    {
        TestGuid = Guid.NewGuid().ToString();
    }
    public string GetGuid()
    {
        return TestGuid;
    }
}
