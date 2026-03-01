using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitFlow.Transport;

public sealed class RabbitMqConsumer(IConnection connection, string queueName) : IMessageConsumer,  IDisposable, IAsyncDisposable
{
    private IChannel? _channel;
    
    public async Task StartAsync(Func<TransportMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken)
    {
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var transportMessage = new TransportMessage
            {
                Body = eventArgs.Body.ToArray(),
                Headers = eventArgs.BasicProperties.Headers ?? new Dictionary<string, object?>(),
                Queue = queueName,
                Acknowledger = new RabbitMqAcknowledger(_channel, eventArgs.DeliveryTag)
            };

            await onMessage(transportMessage, cancellationToken);
        };
        
        await _channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        connection.Dispose();
        _channel?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
        
        if (_channel != null) 
            await _channel.DisposeAsync();
    }
}