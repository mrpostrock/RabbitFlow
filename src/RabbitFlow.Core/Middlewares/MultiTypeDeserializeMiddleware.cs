using System.Text;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class MultiTypeDeserializeMiddleware(
    IDictionary<string, Type> typeMap,
    IMessageSerializer serializer,
    string itemsKey = "message")
    : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken)
    {
        var containsMessageTypeHeader = context.Transport.Headers.TryGetValue("message-type", out var type);
        if (!containsMessageTypeHeader || type == null)
            throw new InvalidOperationException("Cannot find the message type header");

        var typeKey = Encoding.UTF8.GetString((byte[])type);
        var typeRegistered = typeMap.TryGetValue(typeKey, out var targetType);
        if (typeRegistered && targetType is not null)
        {
            var obj = serializer.Deserialize(context.Transport.Body, targetType);
            context.Items[itemsKey] = obj;
        }
        else
        {
            throw new InvalidOperationException($"Unknown message type: {typeKey}");
        }

        await next(context, cancellationToken);
    }
}