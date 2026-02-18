using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Tests.Fakes;

public class FakeAckHandle : IAckHandle
{
    public Task AckAsync()
    {
        return Task.CompletedTask;
    }

    public Task NackAsync(bool requeue)
    {
        return Task.CompletedTask;
    }
}