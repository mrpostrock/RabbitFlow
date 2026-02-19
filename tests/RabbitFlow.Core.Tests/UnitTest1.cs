using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RabbitFlow.Core.Extensions;
using RabbitFlow.Core.Interfaces;
using RabbitFlow.Core.Middlewares;
using RabbitFlow.Core.Tests.Fakes;

namespace RabbitFlow.Core.Tests;

public class Tests
{
    private MessagePipeline _pipeline;
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
     
        var builder = new MessagePipelineBuilder(services);
        builder.Use<ErrorHandlingMiddleware>();
        builder.Use<LoggingMiddleware>();
        builder.Use<MessageTypeMiddleware>();

        var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "MyRecord", typeof(MyRecord) },
            { "Order", typeof(Order) }
        };
        
        builder.UseJson(typeMap);
        builder.Handle<Order, FakeHandler>();
        
        var provider = services.BuildServiceProvider();
        _pipeline = builder.Build(provider);
    }

    [Test]
    public async Task Test1()
    {
        await _pipeline.ExecuteAsync(new MessageContext
        {
            Ack = new FakeAckHandle(),
            Body = JsonSerializer.SerializeToUtf8Bytes(new MyRecord("Maksim", "Malenda")),
            Headers = { new KeyValuePair<string, object>("message-type", "MyRecord") },
            CancellationToken = CancellationToken.None,
        });
        
        await _pipeline.ExecuteAsync(new MessageContext
        {
            Ack = new FakeAckHandle(),
            Body = JsonSerializer.SerializeToUtf8Bytes(new Order(100500)),
            Headers = { new KeyValuePair<string, object>("message-type", "Order") },
            CancellationToken = CancellationToken.None,
        });
    }
}   