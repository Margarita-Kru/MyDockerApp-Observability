using Microsoft.AspNetCore.Mvc;
using MyDockerApp.Dtos;
using System.Text.Json;

namespace MyDockerApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExternalApiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExternalApiController> _logger;
        public ExternalApiController(IHttpClientFactory httpClientFactory, ILogger<ExternalApiController> logger)
        {
            _httpClientFactory = httpClientFactory; 
            _logger = logger;
        }
        [HttpGet("external-user")]
        public async Task<IActionResult> GetExternalUser()
        {
            var client = _httpClientFactory.CreateClient("JsonPlaceholderClient"); //внедряем HttpClient

            var response = await client.GetAsync("users/1"); //делаем Get-запрос

            if (!response.IsSuccessStatusCode) return StatusCode(503); //проверяем, что статус ответа 200

            try
            {
                var user = await response.Content.ReadFromJsonAsync<ExternalUserDto>(); //читаем и десериализируем json
                if (user == null)
                {
                    return NotFound("Не удалось прочитать данные");
                }

                return Ok(user); //возвращаем JSON клиенту
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Внешнее API вернуло невалидный JSON");
                return StatusCode(503);
            }
        }
    }
}
