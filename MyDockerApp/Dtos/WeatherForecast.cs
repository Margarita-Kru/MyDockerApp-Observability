using System.ComponentModel.DataAnnotations;

namespace MyDockerApp.Dtos;

public record WeatherForecast(DateOnly Date,
    [Range(-60,60, ErrorMessage = "Температура должна быть от -60 до 60 градусов")]
    int TemperatureC,
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Описание должно быть от 3 до 50 символов")]
    string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
