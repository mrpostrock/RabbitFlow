using System.Threading.Channels;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Runtime;

public class WorkersProcessingRuntime(IMessageConsumer messageConsumer, MessagePipeline pipeline)
{
    private readonly RuntimeOptions _runtimeOptions = new();
    
    private Channel<TransportMessage> _channel = null!;
    private readonly List<Task> _workers = [];
    private readonly CancellationTokenSource _cts = new();
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = Channel.CreateBounded<TransportMessage>(
            new BoundedChannelOptions(_runtimeOptions.ChannelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

        StartWorkers();

        messageConsumer.StartAsync(async (message, token) =>
            {
                await _channel.Writer.WriteAsync(message, token);
            },
            cancellationToken
        );

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopWorkers();
    }

    private async Task StopWorkers()
    {
        await _cts.CancelAsync();
    }

    private void StartWorkers()
    {
        for (var i = 0; i < _runtimeOptions.WorkersAmount; i++)
            _workers.Add(Task.Run(() => WorkerLoop(_cts.Token), _cts.Token));
    }
    
    private async Task WorkerLoop(CancellationToken ct)
    {
        var workerId = Guid.NewGuid().ToString();
        
        await foreach (TransportMessage message in _channel.Reader.ReadAllAsync(ct))
        {
            if (ct .IsCancellationRequested)
                break;
            
            try
            {
                await ProcessMessageAsync(message, ct, workerId);
                await message.Acknowledger.AckAsync();
            }
            catch (Exception ex)
            {
                await message.Acknowledger.NackAsync(requeue: false);
            }
        }
    }
    
    private async Task ProcessMessageAsync(
        TransportMessage message,
        CancellationToken ct,
        string workerId)
    {
        var context = new MessageContext
        {
            Transport = message,
            Items = { {"workerId", workerId} }
        };

        await pipeline.ExecuteAsync(context, ct);
    }
}

internal class RuntimeOptions
{
    public int WorkersAmount { get; set; } = 10;
    public int ChannelCapacity { get; set; } = 100;
}