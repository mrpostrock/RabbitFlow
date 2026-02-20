using Microsoft.Extensions.Logging;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next)
    {
        logger.LogInformation("Executing message");
        
        await next(context);
        
        logger.LogInformation("Executed message");
    }
}