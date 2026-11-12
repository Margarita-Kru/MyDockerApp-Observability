using MyDockerApp.Interfaces;

namespace MyDockerApp.Services;

public class SingletonService : ISingletonService //делаем иммутабельным 
{
    private string TestGuid { get; } = string.Empty;

    public SingletonService()
    {
        TestGuid = Guid.NewGuid().ToString();
    }

    public string GetGuid()
    {
        return TestGuid;
    }
}
