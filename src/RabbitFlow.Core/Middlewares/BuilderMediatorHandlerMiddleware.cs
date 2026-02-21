using Microsoft.Extensions.DependencyInjection;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class BuilderMediatorHandlerMiddleware(IServiceProvider serviceProvider, IReadOnlyDictionary<Type, Func<IServiceProvider, object, MessageContext, CancellationToken, Task>> handlerRegistry, string itemsKey = "message") : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken)
    {
        if (!context.Items.TryGetValue(itemsKey, out var msg))
            throw new InvalidOperationException("Message not found in context");
        
        var messageType = msg.GetType();

        if (!handlerRegistry.TryGetValue(messageType, out var handlerType))
            throw new InvalidOperationException($"No handler registered for {messageType}");
        
        using var scope = serviceProvider.CreateScope();
        await handlerType.Invoke(scope.ServiceProvider, msg, context, cancellationToken);

        await next(context, cancellationToken);
    }
}