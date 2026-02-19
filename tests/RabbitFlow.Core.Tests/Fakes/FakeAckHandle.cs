using RabbitFlow.Core.Interfaces;
using RabbitFlow.Transport;

namespace RabbitFlow.Core.Tests.Fakes;

public class FakeAckHandle : IMessageAcknowledger
{
    public ValueTask AckAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask NackAsync(bool requeue)
    {
        return ValueTask.CompletedTask;
    }
}