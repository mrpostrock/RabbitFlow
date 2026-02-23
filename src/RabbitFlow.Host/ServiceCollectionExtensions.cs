using System.Threading.Channels;
using RabbitFlow.Core;
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
                serviceCollection.AddSingleton(serviceProvider =>
                {
                    var consumer = new RabbitMqConsumer(serviceProvider.GetRequiredService<IConnection>(), queue.QueueName);
                    var pipeline = queue.Pipeline.Build(serviceProvider);
                    var logger = serviceProvider.GetRequiredService<ILogger<WorkersProcessingRuntime>>();
                    
                    return new WorkersProcessingRuntime(consumer, pipeline, logger);
                });
            }
            
            return serviceCollection;
        }
    }
}