using RabbitFlow.Runtime;

namespace RabbitFlow.Host;

public class Worker(ILogger<Worker> logger, IEnumerable<MessageProcessingRuntime> runtimes) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var t = runtimes;
        var result = t.First();
        
        await result.StartAsync(cancellationToken: stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}