using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core;

public sealed class TransportMessage
{
    public required ReadOnlyMemory<byte> Body { get; init; }

    public required IDictionary<string, object?> Headers { get; init; }

    public required string Queue { get; init; }

    public required IMessageAcknowledger Acknowledger { get; init; }
}
