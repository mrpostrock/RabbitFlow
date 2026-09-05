namespace RabbitFlow.Core.Interfaces;

public interface IMessageHandler<in TMessage>
{
    Task HandleAsync(TMessage message, MessageContext context, CancellationToken cancellationToken);
}