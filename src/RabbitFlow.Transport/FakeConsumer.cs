using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Transport;

public class FakeConsumer : IMessageConsumer
{
    public async Task StartAsync(Func<TransportMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await onMessage(new TransportMessage
                {
                    Body = "{\"id\":1}"u8.ToArray(),
                    Headers = new Dictionary<string, object>()
                    {
                        {"message-type", "Order"u8.ToArray()}
                    },
                    Queue = "test-queue",
                    Acknowledger = new FakeAck()
                },
                cancellationToken);

            await Task.Delay(100, cancellationToken);
        }
    }
}