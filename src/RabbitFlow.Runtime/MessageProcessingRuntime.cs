using Microsoft.Extensions.DependencyInjection;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Runtime;

public class MessageProcessingRuntime(
    IMessageConsumer messageConsumer,
    MessagePipeline pipeline)
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return messageConsumer.StartAsync(ProcessAsync, cancellationToken);
    }

    private async Task ProcessAsync(TransportMessage message, CancellationToken token)
    {
        var context = new MessageContext
        {
            Transport = message
        };

        try
        {
            await pipeline.ExecuteAsync(context);
            await message.Acknowledger.AckAsync();
        }
        catch (Exception e)
        {
            context.MarkAsFailed(e);
            await message.Acknowledger.NackAsync(requeue: false);
        }
    }
}