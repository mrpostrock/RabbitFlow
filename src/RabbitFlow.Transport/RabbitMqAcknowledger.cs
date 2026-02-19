using RabbitMQ.Client;

namespace RabbitFlow.Transport;

public class RabbitMqAcknowledger(IChannel channel, ulong deliveryTag) : IMessageAcknowledger 
{
    public async ValueTask AckAsync()
    {
        await channel.BasicAckAsync(deliveryTag: deliveryTag, false);
    }

    public async ValueTask NackAsync(bool requeue)
    {
        await channel.BasicNackAsync(deliveryTag: deliveryTag,false, requeue);
    }
}