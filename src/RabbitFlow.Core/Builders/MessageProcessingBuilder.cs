using Microsoft.Extensions.DependencyInjection;

namespace RabbitFlow.Core.Builders;

public class MessageProcessingBuilder(IServiceCollection serviceCollection)
{
    private readonly List<QueueBuilder> _queues = [];
    private readonly MessagePipelineBuilder _pipelineBuilder = new(serviceCollection);

    public MessageProcessingBuilder AddQueue(
        string queueName,
        Action<QueueBuilder> configure)
    {
        var queue = new QueueBuilder(queueName, _pipelineBuilder);
        configure(queue);
        _queues.Add(queue);
        
        return this;
    }
    
    public IReadOnlyList<QueueBuilder> Build() => _queues;
}