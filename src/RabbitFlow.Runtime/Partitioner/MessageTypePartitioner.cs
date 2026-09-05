using RabbitFlow.Core;
using RabbitFlow.Core.Extensions;

namespace RabbitFlow.Runtime.Partitioner;

public class MessageTypePartitioner(Dictionary<string, int> typeToPartition) : IPartitioner<TransportMessage>
{
    public int PartitionCount { get; } = typeToPartition.Values.Max() + 1;

    public int GetPartition(TransportMessage message)
    {
        if (!message.Headers.TryGetValue("message-type", out _))
            return 0;
        
        var bytes = message.Headers["message-type"] as byte[] ?? [];
        var messageType = bytes.ToReadableString();
        
        if (!typeToPartition.TryGetValue(messageType, out var partition))
            throw new InvalidOperationException($"Unknown message type: {messageType}");

        return partition;
    }
}