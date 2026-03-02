using System.Text;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Serializers;

namespace RabbitFlow.Core.Middlewares;

public class MultiTypeDeserializeMiddleware(IDictionary<Type, IMessageSerializer> serializersRegistry, string itemsKey = "message")
    : IMessageMiddleware
{
    private readonly Dictionary<string, Type> _registry = serializersRegistry.ToDictionary(x => x.Key.Name, x => x.Key, StringComparer.InvariantCultureIgnoreCase);
    
    public async Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken)
    {
        if (!context.Transport.Headers.TryGetValue("message-type", out var transportHeader))
            throw new InvalidOperationException($"Unknown message type: {context.Transport.Headers["message-type"]}");

        var headerValue = string.Empty;
        if (transportHeader is byte[] transportHeaderBytes)
            headerValue = Encoding.UTF8.GetString(transportHeaderBytes);

        _registry.TryGetValue(headerValue, out var messageType);
        if(messageType is null)
            throw new InvalidOperationException($"Unknown message type: {headerValue}");
        
        serializersRegistry.TryGetValue(messageType, out var serializer);
        if (serializer is null)
            throw new InvalidOperationException($"Unknown message type: {headerValue}");

        var obj = serializer.Deserialize(context.Transport.Body, messageType);
        context.Items[itemsKey] = obj;

        await next(context, cancellationToken);
    }
}