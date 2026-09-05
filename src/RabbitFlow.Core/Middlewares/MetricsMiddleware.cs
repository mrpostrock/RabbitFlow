using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class MetricsMiddleware : IMessageMiddleware
{
    private readonly ILogger<MetricsMiddleware> _logger;

    private long _processedMessages;
    private long _failedMessages;
    private long _totalProcessingMilliseconds;
    private long _inFlight;

    private readonly Timer _timer;

    public MetricsMiddleware(ILogger<MetricsMiddleware> logger)
    {
        _logger = logger;

        _timer = new Timer(
            LogMetrics,
            null,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10));
    }
    
    public async Task InvokeAsync(
        MessageContext context,
        MessageDelegate next,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _inFlight);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context, cancellationToken);

            Interlocked.Increment(ref _processedMessages);
        }
        catch
        {
            Interlocked.Increment(ref _failedMessages);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            Interlocked.Add(ref _totalProcessingMilliseconds, stopwatch.ElapsedMilliseconds);
            Interlocked.Decrement(ref _inFlight);
        }
    }
    
    private void LogMetrics(object? state)
    {
        var processed = Interlocked.Exchange(ref _processedMessages, 0);
        var failed = Interlocked.Exchange(ref _failedMessages, 0);
        var totalMs = Interlocked.Exchange(ref _totalProcessingMilliseconds, 0);
        var inFlight = Interlocked.Read(ref _inFlight);

        var averageMs = processed == 0
            ? 0
            : totalMs / processed;

        _logger.LogInformation(
            """
            Message metrics:
            MPS: {Mps}
            Avg processing time: {AverageMs} ms
            Failed: {Failed}
            In-flight: {InFlight}
            """,
            processed / 10.0,
            averageMs,
            failed,
            inFlight);
    }
}
