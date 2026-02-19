using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Core.Middlewares;

public class HandlerMiddleware<T>(Func<T, MessageContext, Task> handler) : IMessageMiddleware
{
    public async Task InvokeAsync(MessageContext context, MessageDelegate next)
    {
        await handler((T)context.Items["message"]!, context);
    }
}