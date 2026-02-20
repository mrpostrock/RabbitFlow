using Microsoft.Extensions.Logging;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Domain;

namespace RabbitFlow.Application;

public class OrderHandler(ILogger<OrderHandler> logger, ScopeMarker marker) : IMessageHandler<Order>
{
    public Task HandleAsync(Order message, MessageContext context)
    {
        logger.LogInformation("Order handler started, scoped marker {id}", marker.Id);
        return Task.CompletedTask;
    }
}