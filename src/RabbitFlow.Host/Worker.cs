using RabbitFlow.Runtime;

namespace RabbitFlow.Host;

public class Worker(ILogger<Worker> logger, IEnumerable<WorkersProcessingRuntime> runtimes) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workersProcessingRuntimes = runtimes.ToArray();
        
        await workersProcessingRuntimes[0].StartAsync(stoppingToken);
        
        await Task.Delay(10000, stoppingToken);
        
        await workersProcessingRuntimes[1].StartAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
        
        await workersProcessingRuntimes[1].StopAsync(stoppingToken);
        await workersProcessingRuntimes[0].StopAsync(stoppingToken);
    }
}