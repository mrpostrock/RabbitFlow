using Microsoft.Extensions.Logging;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Domain;

namespace RabbitFlow.Application;

public class OrderHandler(ILogger<OrderHandler> logger) : IMessageHandler<Order>
{
    public async Task HandleAsync(Order message, MessageContext context)
    {
        logger.LogInformation("Order Received with id {id}", message.Id);
        await Task.Delay(15000);
        logger.LogInformation("Order processed with id {id}", message.Id);
    }
}


public class UserHandler(ILogger<UserHandler> logger) : IMessageHandler<User>
{
    public async Task HandleAsync(User message, MessageContext context)
    {
        logger.LogInformation("User received with id {id}", message.Id);
        
        await Task.Delay(10000);
        
        logger.LogInformation("User processed with id {id}", message.Id);
    }
}