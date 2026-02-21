using Microsoft.Extensions.Logging;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["workerId"] = context.Items["workerId"]
        };

        using var loggerScope = logger.BeginScope(scopeData);
        await next(context, cancellationToken);
    }
}