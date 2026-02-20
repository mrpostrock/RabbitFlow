using Microsoft.Extensions.Logging;

namespace RabbitFlow.Core;

public class ScopeMarker(ILogger<ScopeMarker> logger) : IDisposable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public void Dispose()
    {
        logger.LogInformation("Scope marker disposed");
    }
}