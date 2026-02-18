namespace RabbitFlow.Core.Interfaces;

public interface IAckHandle
{
    Task AckAsync();
    Task NackAsync(bool requeue);
}