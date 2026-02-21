namespace RabbitFlow.Core;

public delegate Task MessageDelegate(MessageContext context, CancellationToken cancellationToken = default );