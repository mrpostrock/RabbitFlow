using System.Threading.Channels;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Transport;

public class FakeConsumer(string queueName, Channel<TransportMessage> channel) : IMessageConsumer
{
    public async Task StartAsync(Func<TransportMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken)
    {
        await foreach (var message in channel.Reader.ReadAllAsync(cancellationToken))
        {
            var newMessage = new TransportMessage
            {
                Body = message.Body,
                Headers = message.Headers,
                Queue = queueName,
                Acknowledger = message.Acknowledger
            };

            await onMessage(newMessage,  cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}