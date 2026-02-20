using Microsoft.Extensions.DependencyInjection;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class BuilderMediatorHandlerMiddleware(IServiceProvider serviceProvider, IReadOnlyDictionary<Type, Type> handlerRegistry, string itemsKey = "message") : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next)
    {
        if (!context.Items.TryGetValue(itemsKey, out var msg))
            throw new InvalidOperationException("Message not found in context");
        
        var messageType = msg.GetType();

        if (!handlerRegistry.TryGetValue(messageType, out var handlerType))
            throw new InvalidOperationException($"No handler registered for {messageType}");
        
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod("HandleAsync")!;
        await (Task)method.Invoke(handler, [msg, context])!;

        await next(context);
    }
}