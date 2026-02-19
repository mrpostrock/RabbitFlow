using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core;

public sealed class MessagePipeline
{
    private readonly MessageDelegate _pipeline;

    public MessagePipeline(IEnumerable<IMessageMiddleware> middlewares)
    {
        MessageDelegate pipeline = _ => Task.CompletedTask;

        foreach (var m in middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = ctx => m.InvokeAsync(ctx, next);
        }
        
        _pipeline = pipeline;
    }
    
    public Task ExecuteAsync(MessageContext context) => _pipeline(context);
}