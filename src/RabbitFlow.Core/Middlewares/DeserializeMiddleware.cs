using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class DeserializeMiddleware<T>(IMessageSerializer serializer, string itemsKey = "message") : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next)
    {
        var obj = serializer.Deserialize(context.Transport.Body, typeof(T));
        context.Items[itemsKey] = obj;

        await next(context);
    }
}