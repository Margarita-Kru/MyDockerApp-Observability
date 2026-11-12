using MyDockerApp.Interfaces;

namespace MyDockerApp.Services;

public class TransientService : ITransientService
{
    private string TestGuid { get; set; } = string.Empty;

    public TransientService()
    {
        TestGuid = Guid.NewGuid().ToString();
    }
    public string GetGuid()
    {
        return TestGuid;
    }
}
