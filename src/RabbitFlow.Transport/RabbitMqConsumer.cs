using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitFlow.Transport;

public sealed class RabbitMqConsumer(IConnection connection, string queueName, uint prefetchCount) : IMessageConsumer,  IDisposable, IAsyncDisposable
{
    private IChannel? _channel;
    
    public async Task StartAsync(Func<TransportMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken)
    {
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.BasicQosAsync(0, (ushort)prefetchCount, false, cancellationToken);
        
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
        
        var consumerTag = await _channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken: cancellationToken);

        cancellationToken.Register(() =>
        {
            _ = Task.Run(async () => await _channel.BasicCancelAsync(consumerTag, cancellationToken: CancellationToken.None), CancellationToken.None);
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);
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