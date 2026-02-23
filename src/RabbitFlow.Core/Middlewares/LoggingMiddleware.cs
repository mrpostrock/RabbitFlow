using System.Text;
using Microsoft.Extensions.Logging;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next, CancellationToken cancellationToken)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["worker-id"] = context.Items["workerId"],
            ["message-id"] = GetString(context.Transport.Headers["message-id"]),
            ["raw-message"] = GetString(context.Transport.Headers["raw-message"])
        };

        using var loggerScope = logger.BeginScope(scopeData);
        await next(context, cancellationToken);
    }

    private string GetString(object header)
    {
        var bytes = (byte[]) header;
        return Encoding.UTF8.GetString(bytes);
    }
}