using System.Text;
using Microsoft.Extensions.Logging;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken)
    {
        context.Transport.Headers.TryGetValue("message-id", out var messageId);
        
        var scopeData = new Dictionary<string, object>
        {
            ["worker-id"] = context.Items["workerId"],
            ["message-id"] = GetString(messageId) ?? Guid.NewGuid().ToString()
        };

        using var loggerScope = logger.BeginScope(scopeData);
        await next(context, cancellationToken);
    }

    private static string? GetString(object? header)
    {
        if (header == null)
            return null;
        
        var bytes = (byte[]) header;
        return Encoding.UTF8.GetString(bytes);
    }
}