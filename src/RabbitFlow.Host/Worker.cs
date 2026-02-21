using System.Text;
using System.Threading.Channels;
using RabbitFlow.Core;
using RabbitFlow.Runtime;
using RabbitFlow.Transport;

namespace RabbitFlow.Host;

public class Worker(ILogger<Worker> logger, IEnumerable<WorkersProcessingRuntime> runtimes, Channel<TransportMessage> channel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workersProcessingRuntimes = runtimes.ToArray();
        await workersProcessingRuntimes[0].StartAsync(stoppingToken);
        
        await PublishMessageAsync(stoppingToken);
    }
    
    private Task PublishMessageAsync(CancellationToken cancellationToken)
    {
        const string templateOrder = """
                                     { 
                                         "id": @id
                                     }
                                     """;

        const string templateUser = """
                                    {
                                        "id": "@id",
                                        "name": "@name"
                                    }
                                    """;

        Task.Run(async () =>
        {
            var random = new Random();
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var a = new TransportMessage
                {
                    Body = Encoding.UTF8.GetBytes(templateOrder.Replace("@id", random.Next().ToString())),
                    Headers = new Dictionary<string, object>()
                    {
                        { "message-type", "Order"u8.ToArray() }
                    },
                    Queue = "tests-queue",
                    Acknowledger = new FakeAck()
                };

                var b = new TransportMessage
                {
                    Body = Encoding.UTF8.GetBytes(templateUser.Replace("@name", "Maksim")
                        .Replace("@id", Guid.NewGuid().ToString())),
                    Headers = new Dictionary<string, object>
                    {
                        { "message-type", "User"u8.ToArray() }
                    },
                    Queue = "tests-queue",
                    Acknowledger = new FakeAck()
                };
                
                
                await channel.Writer.WriteAsync(DateTime.UtcNow.Microsecond % 3 == 0 ? a : b, cancellationToken);
            }
        }, cancellationToken);
        
        return Task.CompletedTask;
    }
}