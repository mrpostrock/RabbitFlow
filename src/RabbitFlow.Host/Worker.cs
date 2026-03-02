using RabbitFlow.Runtime;

namespace RabbitFlow.Host;

public class Worker(ILogger<Worker> logger, WorkersProcessingRuntime runtime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await runtime.StartAsync(stoppingToken);
    }
}