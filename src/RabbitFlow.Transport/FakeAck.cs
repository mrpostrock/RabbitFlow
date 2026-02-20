using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Transport;

public class FakeAck : IMessageAcknowledger
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