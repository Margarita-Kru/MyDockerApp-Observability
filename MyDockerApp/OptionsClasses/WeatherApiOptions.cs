using System.ComponentModel.DataAnnotations;

namespace MyDockerApp.OptionsClasses;

public class WeatherApiOptions
{
    [Required(ErrorMessage = "ApiKey обязателен!")]
    public string ApiKey { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "MaxItems должен быть от 1 до 10")]
    public int MaxItems { get; set; }
}
