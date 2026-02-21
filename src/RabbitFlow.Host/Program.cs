using RabbitFlow.Application;
using RabbitFlow.Core;
using RabbitFlow.Core.Extensions;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Core.Middlewares;
using RabbitFlow.Domain;
using RabbitFlow.Host;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<IConnection>(_ =>
{
    var factory = new ConnectionFactory();
    factory.UserName = "sbdev";
    factory.Password = "sbdev";

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddTransient<IMessageSerializer, SystemJsonMessageSerializer>();
builder.Services.AddTransient<ScopeMarker>();


builder.Services.AddMessageProcessing(cfg =>
{
    var typeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
    {
        { "Order", typeof(Order) }
    };
    
    cfg.AddRabbitMqQueue("test-queue", queueBuilder =>
    {
        queueBuilder.Pipeline
            .Use<ErrorHandlingMiddleware>()
            .Use<LoggingMiddleware>()
            .Use<MessageTypeMiddleware>()
            .UseSystemJson(typeMap)
            .Handle<Order, OrderHandler>();
    });

    cfg.AddRabbitMqQueue("test-queue2", queueBuilder =>
    {
        queueBuilder.Pipeline
            .Use<ErrorHandlingMiddleware>()
            .Use<LoggingMiddleware>()
            .Use<MessageTypeMiddleware>()
            .UseSystemJson(typeMap)
            .Handle<Order, OrderHandler>();
    });
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();