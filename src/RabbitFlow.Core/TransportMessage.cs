namespace RabbitFlow.Transport;

public sealed class TransportMessage
{
    public required ReadOnlyMemory<byte> Body { get; init; }

    public required IDictionary<string, object> Headers { get; init; }

    public required string Topic { get; init; } // queue / topic / subject

    public required IMessageAcknowledger Acknowledger { get; init; }
}
