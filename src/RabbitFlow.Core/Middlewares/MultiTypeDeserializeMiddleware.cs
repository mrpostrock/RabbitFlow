using System.Text;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class MultiTypeDeserializeMiddleware : IMessageMiddleware
{
    private readonly IDictionary<string, Type> _typeMap;
    private readonly IMessageSerializer _serializer;
    private readonly string _itemsKey;

    public MultiTypeDeserializeMiddleware(
        IDictionary<string, Type> typeMap,
        IMessageSerializer serializer,
        string itemsKey = "message")
    {
        _typeMap = typeMap;
        _serializer = serializer;
        _itemsKey = itemsKey;
    }

    public async Task InvokeAsync(MessageContext context, MessageDelegate next)
    {
        if (context.Transport.Headers.TryGetValue("message-type", out var type) && _typeMap.TryGetValue(Encoding.UTF8.GetString((byte[])type), out var targetType))
        {
            
            var obj = _serializer.Deserialize(context.Transport.Body, targetType);
            context.Items[_itemsKey] = obj;
        }
        else
        {
            throw new InvalidOperationException($"Unknown message type: {context.Transport.Headers["message-type"]}");
        }

        await next(context);
    }
}