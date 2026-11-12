using Confluent.Kafka;

namespace MyDockerApp.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<Null, string> _consumer;
        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:29092",
                GroupId = "weather-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) //точка входа, которую фреймворк вызывает один раз при старте приложения
        {
            _consumer.Subscribe("weather-events");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        _logger.LogInformation("Получено сообщение: {Message}", consumeResult.Message.Value);
                    }
                    catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        // Топика пока нет, ждем немного и пробуем снова
                        _logger.LogWarning("Топик 'weather-events' еще не создан. Ожидание...");
                        await Task.Delay(2000, stoppingToken);
                    }
                }
            }
            catch(OperationCanceledException) { }
            finally
            {
                _consumer.Close(); // Обязательно закрываем соединение
            }
        }
    }
}
