using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class MessageTypeMiddleware : IMessageMiddleware
{
    private const string MessageTypeKey = "MessageType";

    public async Task InvokeAsync(MessageContext context, MessageDelegate next)
    {
        if (context.Transport.Headers.TryGetValue("message-type", out var type))
            context.Items[MessageTypeKey] = type;

        await next(context);
    }
}