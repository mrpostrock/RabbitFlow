using RabbitFlow.Runtime;

namespace RabbitFlow.Host;

public class Worker(ILogger<Worker> logger, WorkersProcessingRuntime processingRuntime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromSeconds(10));
        
        await processingRuntime.StartAsync(stoppingToken);
    }
}