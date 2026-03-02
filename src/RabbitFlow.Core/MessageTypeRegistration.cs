using RabbitFlow.Serializers;

namespace RabbitFlow.Core;

public sealed class MessageTypeRegistration
{
    public string MessageType { get; init; } = null!;
    public Type TargetType { get; init; } = null!;
    public Func<IMessageSerializer, object> Factory { get; init; } = null!;
}