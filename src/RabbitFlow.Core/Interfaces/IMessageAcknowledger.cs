namespace RabbitFlow.Core.Interfaces;

public interface IMessageAcknowledger
{
    ValueTask AckAsync();
    ValueTask NackAsync(bool requeue);
}