namespace RabbitFlow.Core.Interfaces;

public interface IMessageMiddleware
{
    Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken);
}