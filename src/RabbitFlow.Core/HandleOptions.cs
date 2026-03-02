using RabbitFlow.Serializers;
using RabbitFlow.Serializers.SystemJson;

namespace RabbitFlow.Core;

public class HandleOptions<TMessage>
{
    internal IMessageSerializer _messageSerializer { get; private set; }
    
    
    public void UseSystemJson()
    {
        _messageSerializer = new SystemJsonMessageSerializer();
    }

    public void UseNewtonSoftJson()
    {
        _messageSerializer = null!;
    }
}