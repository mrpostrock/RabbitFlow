using RabbitFlow.Producer;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<IConnection>(_ =>
{
    var factory = new ConnectionFactory();
    factory.UserName = "guest";
    factory.Password = "guest";

    factory.ClientProvidedName = " default.producer";
    
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

var host = builder.Build();
host.Run();