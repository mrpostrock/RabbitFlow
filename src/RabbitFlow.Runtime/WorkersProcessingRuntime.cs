using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitFlow.Core;
using RabbitFlow.Core.Configuration;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Runtime.Partitioner;

namespace RabbitFlow.Runtime;

public class WorkersProcessingRuntime(
    IMessageConsumer messageConsumer,
    IMessagePipeline pipeline,
    IOptions<RuntimeOptions> runtimeOptions,
    ILogger<WorkersProcessingRuntime> logger)
{
    private readonly RuntimeOptions _runtimeOptions = runtimeOptions.Value;
    private Channel<TransportMessage>[] _channels = null!;
    private readonly List<Task> _workers = [];

    private readonly HashPartitioner<TransportMessage> _partitioner = new(
        runtimeOptions.Value.PartitionersAmount,
        message =>
        {
            var hash = message.GetHashCode();
            return (uint)hash;
        });

    private Task _workersCompletion;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _channels = Enumerable.Range(0, _runtimeOptions.PartitionersAmount)
            .Select(_ => Channel.CreateBounded<TransportMessage>(
                new BoundedChannelOptions((int)(_runtimeOptions.PrefectCount * 1.2))
                {
                    FullMode = BoundedChannelFullMode.Wait
                }))
            .ToArray();

        StartWorkers(cancellationToken);

        await messageConsumer.StartAsync(async (message, token) =>
            {
                if (token.IsCancellationRequested)
                    return;
                
                var partition = _partitioner.GetPartition(message);
                try
                {
                    await _channels[partition].Writer.WriteAsync(message, token);
                }
                catch (OperationCanceledException)
                {
                    logger.LogDebug("Cancellation requested");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error writing message");
                }
            },
            cancellationToken
        );

        await StopRuntimeAsync();
    }

    public async Task StopRuntimeAsync()
    {
        await _workersCompletion;
        await messageConsumer.StopAsync(CancellationToken.None);
        
        logger.LogInformation("Runtime stopped");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void StartWorkers(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _runtimeOptions.PartitionersAmount; i++)
        {
            var partition = i;
            _workers.Add(WorkerLoop(partition, cancellationToken));
        }
        
        _workersCompletion = Task.WhenAll(_workers);
    }

    private async Task WorkerLoop(int partition, CancellationToken cancellationToken)
    {
        var workerId = Guid.NewGuid().ToString();

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Starting worker loop for {workerId}", workerId);
        try
        {

            await foreach (var message in _channels[partition].Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await ProcessMessageAsync(message, workerId, partition, cancellationToken);
                    await message.Acknowledger.AckAsync();
                }
                catch (Exception ex)
                {
                    await message.Acknowledger.NackAsync(requeue: false);
                }
            }
        }
        catch (OperationCanceledException e)
        {
            logger.LogInformation("Worker {partition} finished", partition);
        }
    }

    private async Task ProcessMessageAsync(TransportMessage message, string workerId, int partition, CancellationToken cancellationToken)
    {
        var context = new MessageContext
        {
            Transport = message,
            Items =
            {
                { "workerId", workerId },
                { "partition", partition }
            }
        };

        await pipeline.ExecuteAsync(context, cancellationToken);
    }
}