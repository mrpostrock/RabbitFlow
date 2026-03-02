using RabbitFlow.Core.Interfaces;
using RabbitFlow.Serializers;

namespace RabbitFlow.Core.Middlewares;

public class DeserializeMiddleware(IMessageSerializer serializer, Type messageType, string itemsKey = "message") : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken)
    {
        var obj = serializer.Deserialize(context.Transport.Body, messageType);
        context.Items[itemsKey] = obj;

        await next(context, cancellationToken);
    }
}