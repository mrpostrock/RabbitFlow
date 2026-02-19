using Microsoft.Extensions.Logging;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Tests.Fakes;

public class FakeHandler(ILogger<FakeHandler> logger) : IMessageHandler<Order>
{
    public Task HandleAsync(Order message, MessageContext context)
    {
        logger.LogInformation("Received Order {Message}", message);
        return Task.CompletedTask;
    }
}