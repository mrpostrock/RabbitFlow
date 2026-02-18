using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Core.Middlewares;

namespace RabbitFlow.Core;

public class MessagePipelineBuilder(IServiceCollection services)
{
    private readonly IList<Func<IServiceProvider, IMessageMiddleware>> _factories = new List<Func<IServiceProvider, IMessageMiddleware>>();
    private readonly Dictionary<Type, Type> _handlerRegistry = new();

    public MessagePipelineBuilder Use<TMiddleware>() where TMiddleware : class, IMessageMiddleware
    {
        if (services.All(x => x.ServiceType != typeof(IMessageMiddleware)))
            services.AddTransient<TMiddleware>();
        
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
        services.TryAddTransient(typeof(THandler));
        _handlerRegistry[typeof(TMessage)] = typeof(THandler);

        return this;
    }
    
    public MessagePipeline Build(IServiceProvider serviceProvider)
    {
        var factories = _factories.Select(f => f(serviceProvider)).ToList();
        factories.Add(new BuilderMediatorHandlerMiddleware(serviceProvider, _handlerRegistry));
        
        return new MessagePipeline(factories);
    }
}