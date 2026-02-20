using Microsoft.Extensions.DependencyInjection;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Core.Middlewares;

namespace RabbitFlow.Core.Extensions;

public static class RabbitFlowExtensions
{
    extension(MessagePipelineBuilder builder)
    {
        public MessagePipelineBuilder UseJson<TMessage>(string itemsKey = "message")
        {
            builder.Use(sp => new DeserializeMiddleware<TMessage>(sp.GetRequiredService<IMessageSerializer>(), itemsKey));
            return builder;
        }

        public MessagePipelineBuilder UseJson(IDictionary<string, Type> typeMap,
            string itemsKey = "message")
        {
            builder.Use(sp => new MultiTypeDeserializeMiddleware(
                typeMap,
                sp.GetRequiredService<IMessageSerializer>(),
                itemsKey));

            return builder;
        }
    }
}