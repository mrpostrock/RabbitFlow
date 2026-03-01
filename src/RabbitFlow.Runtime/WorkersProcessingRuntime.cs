using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Runtime.Partitioner;

namespace RabbitFlow.Runtime;

public class WorkersProcessingRuntime(
    IMessageConsumer messageConsumer,
    IMessagePipeline pipeline,
    ILogger<WorkersProcessingRuntime> logger)
{
    private readonly RuntimeOptions _runtimeOptions = new();
    
    private Channel<TransportMessage>[] _channels = null!;
    
    private readonly List<Task> _workers = [];
    private readonly CancellationTokenSource _cts = new();

    private readonly EntityTypePartitioner _partitioner = new(new Dictionary<string, int>
    {
        { "test-entity", 1 },
        { "test-entity2", 2 }
    });
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _channels = Enumerable.Range(0, _runtimeOptions.PartitionersAmount)
            .Select(_ => Channel.CreateBounded<TransportMessage>(
                new BoundedChannelOptions(_runtimeOptions.ChannelCapacity)
                {
                    FullMode = BoundedChannelFullMode.Wait
                }))
            .ToArray();

        StartWorkers();
        
        messageConsumer.StartAsync(async (message, token) =>
            {
                var partition = _partitioner.GetPartition(message);
                await _channels[partition-1].Writer.WriteAsync(message, token);
            },
            cancellationToken
         );

        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        await StopWorkers();
    }

    private async Task StopWorkers()
    {
        await _cts.CancelAsync();
    }

    private void StartWorkers()
    {
        for (var i = 0; i < _runtimeOptions.PartitionersAmount; i++)
        {
            var partition = i;
            _workers.Add(Task.Run(() => WorkerLoop(partition, _cts.Token), _cts.Token));
        }
    }
    
    private async Task WorkerLoop(int partition, CancellationToken ct)
    {
        var workerId = Guid.NewGuid().ToString();
        logger.LogInformation("Starting worker loop for {workerId}", workerId);
        
        await foreach (var message in _channels[partition].Reader.ReadAllAsync(ct))
        {
            if (ct .IsCancellationRequested)
                break;
            
            try
            {
                await ProcessMessageAsync(message, workerId, partition, ct);
                await message.Acknowledger.AckAsync();
            }
            catch (Exception ex)
            {
                await message.Acknowledger.NackAsync(requeue: true);
            }
        }
    }
    
    private async Task ProcessMessageAsync(TransportMessage message,
        string workerId, int partition,
        CancellationToken ct)
    {
        var context = new MessageContext
        {
            Transport = message,
            Items =
            {
                {"workerId", workerId},
                {"partition", partition}
            }
        };

        await pipeline.ExecuteAsync(context, ct);
    }
}

public class RuntimeOptions
{
    public int PartitionersAmount { get; set; } = 2;
    public int ChannelCapacity { get; set; } = 10;
}