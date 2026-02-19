using RabbitFlow.Core.Builders;
using RabbitFlow.Runtime;
using RabbitFlow.Transport;
using RabbitMQ.Client;

namespace RabbitFlow.Host;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddMessageProcessing(Action<MessageProcessingBuilder> configure)
        {
            var builder = new MessageProcessingBuilder(serviceCollection);
            configure(builder);

            var queues = builder.Build();

            foreach (var queue in queues)
            {
                serviceCollection.AddSingleton(sp =>
                {
                    var consumer = new RabbitMqConsumer(sp.GetRequiredService<IConnection>(), queue.QueueName);
                    var pipeline = queue.Pipeline.Build(sp);
                    
                    return new MessageProcessingRuntime(consumer, pipeline);
                });
            }
            
            return serviceCollection;
        }
    }
}