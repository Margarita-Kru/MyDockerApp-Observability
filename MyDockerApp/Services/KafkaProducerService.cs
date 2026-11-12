using Confluent.Kafka;

namespace MyDockerApp.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<Null, string> _producer;
    public KafkaProducerService(IConfiguration configuration)
    {
        var producerConfig = new ProducerConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:29092" };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task SendMessageAsync(string topic, string message)
    {
        await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
    }
}
