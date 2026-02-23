using System.Text;
using System.Text.Json;
using RabbitFlow.Domain;
using RabbitMQ.Client;

namespace RabbitFlow.Producer;

public class Worker(ILogger<Worker> logger, IConnection connection) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using (connection)
        {
            await using (var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken))
            {
                var random = new Random();

                while (!stoppingToken.IsCancellationRequested)
                {
                    var message = new Order(random.Next());
                    var json = JsonSerializer.Serialize(message);

                    await channel.BasicPublishAsync(
                        string.Empty,
                        "rabbit.flow",
                        true,
                        new BasicProperties
                        {
                            Headers = new Dictionary<string, object?>()
                            {
                                { "message-type", nameof(Order) },
                                { "raw-message", json },
                                { "message-id", Guid.NewGuid().ToString() }
                            }
                        },
                        Encoding.UTF8.GetBytes(json),
                        stoppingToken
                    );
                }
            }
        }
    }
}