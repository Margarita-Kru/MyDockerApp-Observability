using MyDockerApp.Dtos;
using MyDockerApp.Entities;

namespace MyDockerApp.Services
{
    public interface IWeatherService
    {
        Task<IEnumerable<WeatherForecast>> GenerateAndSaveWeatherAsync(int maxItems);

        Task<WeatherRecord?> GetByIdAsync(int id);

        Task<WeatherRecord?> UpdateAsync(int id, WeatherForecast updatedForecast);

        Task<WeatherRecord?> DeleteAsync(int id);
    }
}
