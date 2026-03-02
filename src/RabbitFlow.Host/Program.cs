using RabbitFlow.Application;
using RabbitFlow.Core.Middlewares;
using RabbitFlow.Domain;
using RabbitFlow.Extensions.DependencyInjection;
using RabbitFlow.Host;
using RabbitFlow.Serializers;
using RabbitFlow.Serializers.SystemJson;
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
    cfg.AddQueue("rabbit.flow", queueBuilder =>
    {
        queueBuilder.Pipeline
            .Use<LoggingMiddleware>()
            .Use<ErrorHandlingMiddleware>()
            .Use<MessageTypeMiddleware>()
            .Handle<Order, OrderHandler>(handleOptions =>
            {
                handleOptions.UseSystemJson();
            });
    });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();