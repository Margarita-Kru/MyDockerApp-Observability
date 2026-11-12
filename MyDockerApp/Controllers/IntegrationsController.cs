using Microsoft.AspNetCore.Mvc;
using MyDockerApp.Clients;
using MyDockerApp.Dtos;
using System.Text.Json;

namespace MyDockerApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IntegrationsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ReqResClient _reqResClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public IntegrationsController(IHttpClientFactory httpClientFactory, ReqResClient reqResClient)
    {
        _httpClientFactory = httpClientFactory;
        _reqResClient = reqResClient;
    }

    [HttpGet("dog")]
    public async Task<IActionResult> GetRandomDogFromAPI()
    {
        var client = _httpClientFactory.CreateClient("DogClient");
        var response = await client.GetAsync("random");
        if (!response.IsSuccessStatusCode) return NotFound("Dog API недоступен");

        try
        {
            var dog = await response.Content.ReadFromJsonAsync<DogApiResponse>(_jsonOptions);
            if (dog != null)
            {
                return Ok(dog);
            }
            return NotFound("Не удалось прочитать данные");
        } catch (JsonException)
        {
            return NotFound("Dog API вернул невалидный JSON");
        }
    }

    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUserByIdFromAPI(int id)
    {
        var user = await _reqResClient.GetUserAsync(id);
        if (user != null)
        {
            return Ok(user);
        }
        return NotFound("Не удалось прочитать данные");
    }
}
