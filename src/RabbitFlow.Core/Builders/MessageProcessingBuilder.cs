using Microsoft.Extensions.DependencyInjection;

namespace RabbitFlow.Core.Builders;

public class MessageProcessingBuilder(IServiceCollection serviceCollection)
{
    private readonly List<QueueBuilder> _queues = [];

    public MessageProcessingBuilder AddRabbitMqQueue(
        string queueName,
        Action<QueueBuilder> configure)
    {
        var queue = new QueueBuilder(queueName, serviceCollection);
        configure(queue);
        _queues.Add(queue);
        
        return this;
    }
    
    public IReadOnlyList<QueueBuilder> Build() => _queues;
}