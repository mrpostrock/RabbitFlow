namespace RabbitFlow.Core.Configuration;

public class RuntimeOptions
{
    public int PartitionersAmount { get; set; } = 2;
    public int PrefectCount { get; set; } = 10;
}