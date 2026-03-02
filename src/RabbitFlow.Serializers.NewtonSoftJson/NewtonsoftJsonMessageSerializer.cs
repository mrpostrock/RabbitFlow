using System.Text.Json;

namespace RabbitFlow.Serializers.NewtonSoftJson;

public class NewtonsoftJsonMessageSerializer(JsonSerializerOptions? options = null) : IMessageSerializer
{

    public object Deserialize(ReadOnlyMemory<byte> body, Type targetType)
    {
        throw new NotImplementedException();
    }

    public ReadOnlyMemory<byte> Serialize(object message)
    {
        throw new NotImplementedException();
    }
}