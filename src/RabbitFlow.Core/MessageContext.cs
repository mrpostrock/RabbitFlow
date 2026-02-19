using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core;

public sealed class MessageContext
{
    public string MessageId { get; } = Guid.NewGuid().ToString();
    public required IAckHandle Ack { get; init; }
    public required ReadOnlyMemory<byte> Body { get; set; }
    public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
    public CancellationToken CancellationToken { get; init; }
}