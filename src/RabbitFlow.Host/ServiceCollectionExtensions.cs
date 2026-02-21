using System.Threading.Channels;
using RabbitFlow.Core;
using RabbitFlow.Core.Builders;
using RabbitFlow.Runtime;
using RabbitFlow.Transport;

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
                    var fakeConsumer = new FakeConsumer(queue.QueueName,  serviceProvider.GetRequiredService<Channel<TransportMessage>>());
                    var pipeline = queue.Pipeline.Build(serviceProvider);
                    
                    return new WorkersProcessingRuntime(fakeConsumer, pipeline);
                });
            }
            
            return serviceCollection;
        }
    }
}