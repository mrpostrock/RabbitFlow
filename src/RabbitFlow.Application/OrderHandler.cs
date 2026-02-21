using Microsoft.Extensions.Logging;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Domain;

namespace RabbitFlow.Application;

public class OrderHandler(ILogger<OrderHandler> logger) : IMessageHandler<Order>
{
    public async Task HandleAsync(Order message, MessageContext context)
    {
        await Task.Delay(500);
        
        logger.LogInformation("Order Received with id {id}", message.Id);
        logger.LogInformation("Order Received with from queue {queue}", context.Transport.Queue);
    }
}