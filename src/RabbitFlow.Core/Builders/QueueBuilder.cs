using Microsoft.Extensions.DependencyInjection;

namespace RabbitFlow.Core.Builders;

public sealed class QueueBuilder
{
    internal QueueBuilder(string queueName,
        IServiceCollection services, MessagePipelineBuilder pipelineBuilder)
    {
        QueueName = queueName;
        Pipeline = pipelineBuilder;
    }

    public MessagePipelineBuilder Pipeline { get; set; }

    public string QueueName { get; set; }
}