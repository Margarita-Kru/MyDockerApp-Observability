using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyDockerApp.OptionsClasses;

namespace MyDockerApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestOptionsController : ControllerBase
    {
        private readonly IOptions<WeatherApiOptions> _options;
        private readonly IOptionsSnapshot<WeatherApiOptions> _snapshot;
        private readonly IOptionsMonitor<WeatherApiOptions> _monitor;

        public TestOptionsController(IOptions<WeatherApiOptions> options,
            IOptionsSnapshot<WeatherApiOptions> snapshot,
            IOptionsMonitor<WeatherApiOptions> monitor)
        {
            _options = options;
            _snapshot = snapshot;
            _monitor = monitor;
        }
        [HttpGet("check")]
        public IActionResult Check()
        {
            return Ok( new
            {
                FromIOptions = _options.Value.MaxItems,
                FromIOptionsSnapshot = _snapshot.Value.MaxItems,
                FromIOptionsMonitor = _monitor.CurrentValue.MaxItems,
            });
        }
    }
}
