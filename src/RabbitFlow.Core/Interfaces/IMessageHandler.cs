namespace RabbitFlow.Core.Interfaces;

public interface IMessageHandler<TMessage>
{
    Task HandleAsync(TMessage message, MessageContext context);
}