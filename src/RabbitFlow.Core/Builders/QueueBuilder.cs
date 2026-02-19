using Microsoft.Extensions.DependencyInjection;

namespace RabbitFlow.Core.Builders;

public sealed class QueueBuilder
{
    internal QueueBuilder(
        string queueName,
        IServiceCollection services)
    {
        QueueName = queueName;
        Pipeline = new MessagePipelineBuilder(services);
    }

    public MessagePipelineBuilder Pipeline { get; set; }

    public string QueueName { get; set; }
}