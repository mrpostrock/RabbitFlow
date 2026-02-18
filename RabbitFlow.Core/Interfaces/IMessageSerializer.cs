namespace RabbitFlow.Core.Interfaces;

public interface IMessageSerializer
{
    object Deserialize(ReadOnlyMemory<byte> body, Type targetType);
    ReadOnlyMemory<byte> Serialize(object message);
}