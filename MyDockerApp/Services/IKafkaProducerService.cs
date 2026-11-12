namespace MyDockerApp.Services;

public interface IKafkaProducerService
{
    Task SendMessageAsync(string topic, string message);
}
