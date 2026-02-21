using System.Threading.Channels;
using RabbitFlow.Application;
using RabbitFlow.Core;
using RabbitFlow.Core.Extensions;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Core.Middlewares;
using RabbitFlow.Domain;
using RabbitFlow.Host;
using RabbitMQ.Client;
using Serilog;
using Serilog.Formatting.Json;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddSerilog(x => x.Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
);
builder.Services.AddTransient<IConnection>(_ =>
{
    var factory = new ConnectionFactory();
    factory.UserName = "sbdev";
    factory.Password = "sbdev";

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddTransient<IMessageSerializer, SystemJsonMessageSerializer>();
builder.Services.AddMessageProcessing(cfg =>
{
    var typeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
    {
        { nameof(Order), typeof(Order) },
        { nameof(User), typeof(User) }
    };
    
    cfg.AddRabbitMqQueue("test-queue", queueBuilder =>
    {
        queueBuilder.Pipeline
            .Use<ErrorHandlingMiddleware>()
            .Use<LoggingMiddleware>()
            .Use<MessageTypeMiddleware>()
            .UseSystemJson(typeMap)
            .Handle<Order, OrderHandler>()
            .Handle<User, UserHandler>();
            
    });
});

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<Channel<TransportMessage>>(_ => Channel.CreateBounded<TransportMessage>(new BoundedChannelOptions(10000)));

var host = builder.Build();
host.Run();