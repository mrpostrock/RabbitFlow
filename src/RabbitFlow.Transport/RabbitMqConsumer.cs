using RabbitFlow.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitFlow.Transport;

public class RabbitMqConsumer(IConnection connection, string queueName) : IMessageConsumer
{
    public async Task StartAsync(Func<TransportMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken)
    {
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var transportMessage = new TransportMessage
            {
                Body = eventArgs.Body,
                Headers = eventArgs.BasicProperties.Headers ?? new Dictionary<string, object>(),
                Topic = queueName,
                Acknowledger = new RabbitMqAcknowledger(channel, eventArgs.DeliveryTag)
            };

            await onMessage(transportMessage, cancellationToken);
        };
        
        await channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken: cancellationToken);
    }
}