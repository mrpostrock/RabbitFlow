using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core;

public sealed class MessagePipeline : IMessagePipeline
{
    private readonly MessageDelegate _pipeline;

    public MessagePipeline(IEnumerable<IMessageMiddleware> middlewares)
    {
        MessageDelegate pipeline = (_, _) => Task.CompletedTask;

        foreach (var m in middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = (ctx, cancellationToken) => m.InvokeAsync(ctx, next, cancellationToken);
        }
        
        _pipeline = pipeline;
    }
    
    public Task ExecuteAsync(MessageContext context, CancellationToken cancellationToken) => _pipeline(context, cancellationToken);
}

public interface IMessagePipeline
{
    Task ExecuteAsync(MessageContext context, CancellationToken cancellationToken);
}