using RabbitFlow.Core;

namespace RabbitFlow.Runtime.Partitioner;

public class EntityTypePartitioner(Dictionary<string, int> typeToPartition) : IPartitioner<TransportMessage>
{
    public int PartitionCount { get; } = typeToPartition.Values.Max() + 1;

    public int GetPartition(TransportMessage message)
    {
        var type = message.Headers["entityType"]?.ToString();
        if (type == null || !typeToPartition.TryGetValue(type, out var partition))
            throw new InvalidOperationException($"Unknown message type: {type}");

        return partition;
    }
}