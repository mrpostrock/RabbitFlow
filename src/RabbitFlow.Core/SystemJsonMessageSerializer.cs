using System.Text.Json;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core;

public class SystemJsonMessageSerializer(JsonSerializerOptions? options = null) : IMessageSerializer
{
    private readonly JsonSerializerOptions _options = options ?? new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public object Deserialize(ReadOnlyMemory<byte> body, Type targetType)
    {
        return JsonSerializer.Deserialize(body.Span, targetType, _options) ?? throw new InvalidOperationException("Deserialize failed");
    }

    public ReadOnlyMemory<byte> Serialize(object message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(), _options);
    }
}