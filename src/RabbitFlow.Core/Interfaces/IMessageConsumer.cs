namespace RabbitFlow.Core.Interfaces;

public interface IMessageConsumer
{
    Task StartAsync(Func<TransportMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}