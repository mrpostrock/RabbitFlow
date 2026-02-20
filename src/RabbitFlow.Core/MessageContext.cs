namespace RabbitFlow.Core;

public sealed class MessageContext
{
    public string MessageId { get; } = Guid.NewGuid().ToString();
    public required TransportMessage Transport { get; init; }
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
    public Exception? FailureException { get; private set; }
    public void MarkAsFailed(Exception exception) => FailureException = exception;
}