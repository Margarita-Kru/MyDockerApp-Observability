using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MyDockerApp.Dtos;
using MyDockerApp.OptionsClasses;
using MyDockerApp.Services;

namespace MyDockerApp.Controllers;

[Route("[controller]")] // Берет имя класса (WeatherForecast) и делает маршрут /WeatherForecast + включает валидацию модели 
[ApiController] // Говорит, что это API-контроллер (включает автоматическую валидацию и т.д.)
[EnableRateLimiting("token")]
public class WeatherForecastController : ControllerBase
{
    private readonly IOptions<WeatherApiOptions> _options;

    private readonly IWeatherService _weatherService;

    public WeatherForecastController(IOptions<WeatherApiOptions> options,
        IWeatherService weatherService)
    {
        _options = options;
        _weatherService = weatherService;
    }

    [HttpGet("{id}")] //атрибут маршрутизации
    public async Task<IActionResult> GetById(int id)
    {
        var record = await _weatherService.GetByIdAsync(id);
        if (record == null) {
            return NotFound();
        }
        return Ok(record);
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        return await _weatherService.GenerateAndSaveWeatherAsync(_options.Value.MaxItems);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] WeatherForecast updatedForecast)
    {
        var record = await _weatherService.UpdateAsync(id, updatedForecast);
        if (record == null) return NotFound();

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _weatherService.DeleteAsync(id);
        if (record == null) return NotFound();

        return NoContent();
    }

    [HttpGet("throwException")]
    public IActionResult ThrowException()
    {
        // Искусственно выбрасываем исключение (деление на ноль)
        int x = 0;
        int result = 10 / x;
        return Ok(result);
    }
}
