using System.Text;
using RabbitFlow.Core;

namespace RabbitFlow.Runtime.Partitioner;

public class EntityTypePartitioner(Dictionary<string, int> typeToPartition) : IPartitioner<TransportMessage>
{
    public int PartitionCount { get; } = typeToPartition.Values.Max() + 1;

    public int GetPartition(TransportMessage message)
    {
        var type = Encoding.UTF8.GetString(message.Headers["message-type"] as byte[] ?? []);
        
        if (!typeToPartition.TryGetValue(type, out var partition))
            throw new InvalidOperationException($"Unknown message type: {type}");

        return partition;
    }
}