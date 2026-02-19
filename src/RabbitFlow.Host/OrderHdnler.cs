using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Host;

public class OrderHandler(ILogger<OrderHandler> logger) : IMessageHandler<Order>
{
    public Task HandleAsync(Order message, MessageContext context)
    {
        logger.LogInformation("Order handler started");
        return Task.CompletedTask;
    }
}