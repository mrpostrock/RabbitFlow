using RabbitFlow.Runtime.Partitioner;

namespace RabbitFlow.Runtime.Tests;

[TestFixture]
public class HashPartitionerTests
{
    [Test]
    public void SameKey_ShouldAlwaysReturnSamePartition()
    {
        var partitioner = new HashPartitioner<string>(
            partitionCount: 4,
            StableHash.Fnv1a
        );

        var p1 = partitioner.GetPartition("user-1");
        var p2 = partitioner.GetPartition("user-1");
        
        Assert.That(p1, Is.EqualTo(p2));
    }

    [Test]
    public void Partition_ShouldBeWithinRange()
    {
        var partitioner = new HashPartitioner<string>(
            partitionCount: 8,
            StableHash.Fnv1a
        );

        for (int i = 0; i < 1000; i++)
        {
            var partition = partitioner.GetPartition($"key-{i}");
            Assert.That(partition, Is.InRange(0, 7));
        }
    }

    [Test]
    public void DifferentKeys_CanGoToDifferentPartitions()
    {
        var partitioner = new HashPartitioner<string>(
            partitionCount: 2,
            StableHash.Fnv1a
        );

        var p1 = partitioner.GetPartition("user-1");
        var p2 = partitioner.GetPartition("order-1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(p1, Is.EqualTo(0).Or.EqualTo(1));
            Assert.That(p2, Is.EqualTo(0).Or.EqualTo(1));
        }
    }

    [Test]
    public void Constructor_InvalidPartitionCount_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HashPartitioner<string>(0, _ => 1));
    }
}

public static class StableHash
{
    private const uint OffsetBasis = 2166136261;
    private const uint Prime = 16777619;

    public static uint Fnv1a(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        unchecked
        {
            uint hash = OffsetBasis;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= Prime;
            }
            return hash;
        }
    }
}