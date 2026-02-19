namespace RabbitFlow.Transport;

public interface IMessageAcknowledger
{
    ValueTask AckAsync();
    ValueTask NackAsync(bool requeue);
}