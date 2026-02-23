using RabbitFlow.Runtime;

namespace RabbitFlow.Host;

public class Worker(ILogger<Worker> logger, IEnumerable<WorkersProcessingRuntime> runtimes) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workersProcessingRuntimes = runtimes.ToArray();
        await workersProcessingRuntimes[0].StartAsync(stoppingToken);
    }
}