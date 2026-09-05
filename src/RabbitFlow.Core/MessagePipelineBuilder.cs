using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Core.Middlewares;

namespace RabbitFlow.Core;

public class MessagePipelineBuilder(IServiceCollection services)
{
    private readonly List<Func<IServiceProvider, IMessageMiddleware>> _factories = [];
    private readonly Dictionary<Type, Func<IServiceProvider, object, MessageContext, CancellationToken, Task>> _newHandlerRegistry = new();

    public MessagePipelineBuilder Use<TMiddleware>() where TMiddleware : class, IMessageMiddleware
    {
        if (services.Any(x => x.ServiceType == typeof(TMiddleware))) 
            return this;
        
        services.TryAddTransient<TMiddleware>();
        _factories.Add(sp => sp.GetRequiredService<TMiddleware>());

        return this;
    }

    public MessagePipelineBuilder Use(Func<IServiceProvider, IMessageMiddleware> factory)
    {
        _factories.Add(factory);
        return this;
    }

    public MessagePipelineBuilder Handle<TMessage, THandler>() where THandler : class, IMessageHandler<TMessage>
    {
        services.TryAddTransient<IMessageHandler<TMessage>, THandler>();
        
        _newHandlerRegistry[typeof(TMessage)] = async (sp, msg, ctx, ct) =>
        {
            var handler = sp.GetRequiredService<IMessageHandler<TMessage>>();
            await handler.HandleAsync((TMessage)msg, ctx, ct);
        };

        return this;
    }
    
    public MessagePipeline Build(IServiceProvider serviceProvider)
    {
        var factories = _factories.Select(f => f(serviceProvider)).ToList();
        factories.Add(new BuilderMediatorHandlerMiddleware(serviceProvider, _newHandlerRegistry));
        
        return new MessagePipeline(factories);
    }
}