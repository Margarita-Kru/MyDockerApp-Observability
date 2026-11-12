using Microsoft.AspNetCore.Mvc;
using MyDockerApp.Interfaces;

namespace MyDockerApp.Controllers;

[Route("[controller]")]
[ApiController]
public class TestServicesController : ControllerBase
{
    private readonly ILogger<TestServicesController> _logger;
    private readonly ITransientService _transientService;
    private readonly IScopedService _scopedService;
    private readonly ISingletonService _singletonService;


    public TestServicesController(ILogger<TestServicesController> logger,
        ITransientService transientService,
        IScopedService scopedService,
        ISingletonService singletonService)
    {
        _logger = logger;
        _transientService = transientService;
        _scopedService = scopedService;
        _singletonService = singletonService;
    }

    [HttpGet(Name = "GetTestServices")]
    public IEnumerable<string> Get()
    {
        string guidTransient = _transientService.GetGuid();
        string guidScoped = _scopedService.GetGuid();
        string guidSingleton = _singletonService.GetGuid();

        _logger.Log(LogLevel.Information,
            "Текущий Guid из singleton {guidSingleton}. Текущий Guid из transient {guidTransient}. Текущий Guid из scoped {guidScoped}.",
            guidSingleton,
            guidTransient,
            guidScoped);

        return [guidSingleton, guidTransient, guidScoped];
    }
}