namespace RabbitFlow.Runtime.Partitioner;

public class HashPartitioner<TKey> : IPartitioner<TKey>
{
    public HashPartitioner(int partitionCount, Func<TKey, uint> hash)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(partitionCount);
        
        _hash = hash ?? throw new ArgumentNullException(nameof(hash));
        PartitionCount = partitionCount;
    }
    
    public int GetPartition(TKey key)
    {
        var hash = _hash(key);
        return (int)(hash % (uint)PartitionCount);
    }

    public int PartitionCount { get; }
    private readonly Func<TKey, uint> _hash;
}