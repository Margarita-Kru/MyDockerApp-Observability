using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using MyDockerApp.Data;
using MyDockerApp.Services;

namespace MyDockerApp.Tests;

public class WeatherServiceTests
{
    [Fact]
    public async Task GenerateAndSave_Should_Return_Records_And_Send_Kafka()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);

        var kafkaMock = new Mock<IKafkaProducerService>();
        var loggerMock = new Mock<ILogger<WeatherService>>();
        var distributedCache = new Mock<IDistributedCache>();

        var service = new WeatherService(loggerMock.Object, dbContext, distributedCache.Object, kafkaMock.Object);

        //Act - действие
        var result = await service.GenerateAndSaveWeatherAsync(5);

        //Assert - утверждение
        Assert.Equal(5, result.Count()); //проверка сколько записей нам вернут
        Assert.Equal(5, dbContext.WeatherRecords.Count());

        Assert.All(result, record =>
        {
            Assert.NotNull(record.Summary);
            Assert.InRange(record.TemperatureC, -20, 55);
        });

        kafkaMock.Verify(k => k.SendMessageAsync("weather-events", It.IsAny<string>()), Times.Once);
    }
}
