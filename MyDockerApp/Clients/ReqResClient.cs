using MyDockerApp.Dtos;

namespace MyDockerApp.Clients;

public class ReqResClient
{
    private readonly HttpClient _httpClient;
    public ReqResClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ReqResUser?> GetUserAsync(int id)
    {
        var response = await _httpClient.GetAsync($"users/{id}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ReqResUserResponse>();
            return result?.Data;
        }
        return null;
    }
}
