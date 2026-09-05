using System.Threading.Channels;
using Microsoft.Extensions.Options;
using RabbitFlow.Core;
using RabbitFlow.Core.Builders;
using RabbitFlow.Core.Configuration;
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
                    var logger = serviceProvider.GetRequiredService<ILogger<WorkersProcessingRuntime>>();
                    var options = serviceProvider.GetRequiredService<IOptions<RuntimeOptions>>();

                    var prefetchCount = options.Value.PrefectCount * options.Value.PartitionersAmount;
                    var consumer = new RabbitMqConsumer(serviceProvider.GetRequiredService<IConnection>(), queue.QueueName, (ushort)prefetchCount);
                    var pipeline = queue.Pipeline.Build(serviceProvider);
                    
                    return new WorkersProcessingRuntime(consumer, pipeline, options, logger);
                });
            }
            
            return serviceCollection;
        }
    }
}