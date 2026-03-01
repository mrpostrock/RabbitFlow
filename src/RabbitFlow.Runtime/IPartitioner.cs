namespace RabbitFlow.Runtime;

public interface IPartitioner<in TMessage>
{
    /// <summary>
    /// Получить номер partition/воркера для сообщения
    /// </summary>
    int GetPartition(TMessage message);

    /// <summary>
    /// Количество partitions, которые может вернуть GetPartition
    /// </summary>
    int PartitionCount { get; }
}