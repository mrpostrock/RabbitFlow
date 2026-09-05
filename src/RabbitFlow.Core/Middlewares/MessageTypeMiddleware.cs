using System.Text;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class MessageTypeMiddleware : IMessageMiddleware
{
    private const string MessageTypeKey = "MessageType";

    public async Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken)
    {
        if (context.Transport.Headers.TryGetValue("message-type", out var type) && type is not null)
            context.Items[MessageTypeKey] = Encoding.UTF8.GetString((byte[])type);

        await next(context, cancellationToken);
    }
}