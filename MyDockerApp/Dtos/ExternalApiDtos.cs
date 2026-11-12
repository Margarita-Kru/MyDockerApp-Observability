using System.Text.Json.Serialization;

namespace MyDockerApp.Dtos
{
    public class ExternalUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class DogApiResponse
    {
        public string Message { get; set; } = string.Empty; // Ссылка на картинку
        public string Status { get; set; } = string.Empty;
    }

    public class ReqResUserResponse
    {
        public ReqResUser Data { get; set; } = new();
    }

    public class ReqResUser
    {
        //[JsonPropertyName("user_id")] применяем атрибут, если название 
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string First_Name { get; set; } = string.Empty;
        public string Last_Name { get; set; } = string.Empty;
    }
}
