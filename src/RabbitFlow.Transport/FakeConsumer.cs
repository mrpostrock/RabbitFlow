using System.Text;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Transport;

public class FakeConsumer(string queueName) : IMessageConsumer
{
    public async Task StartAsync(Func<TransportMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken)
    {
        ulong index = 0;
        const string template = """
                                { 
                                    "id": @id
                                }
                                """;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await onMessage(new TransportMessage
                {
                    Body = Encoding.UTF8.GetBytes(template.Replace("@id", index.ToString())),
                    Headers = new Dictionary<string, object>()
                    {
                        {"message-type", "Order"u8.ToArray()}
                    },
                    Queue = queueName,
                    Acknowledger = new FakeAck()
                },
                cancellationToken);
            
            index++;

            await Task.Delay(5000, cancellationToken);
        }
    }
}