using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitFlow.Core;
using RabbitFlow.Core.Interfaces;

namespace RabbitFlow.Runtime.Tests;


[TestFixture]
public class WorkersProcessingRuntimeTests
{
    private Mock<IMessageConsumer> _consumerMock = null!;
    private Mock<IMessagePipeline> _pipelineMock = null!;
    private Mock<ILogger<WorkersProcessingRuntime>> _loggerMock = null!;

    private WorkersProcessingRuntime _runtime = null!;

    private Func<TransportMessage, CancellationToken, Task> _handler = null!;
    
    [SetUp]
    public void Setup()
    {
        _consumerMock = new Mock<IMessageConsumer>();
        _pipelineMock = new Mock<IMessagePipeline>();
        _loggerMock = new Mock<ILogger<WorkersProcessingRuntime>>();

        _consumerMock
            .Setup(c => c.StartAsync(It.IsAny<Func<TransportMessage, CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Callback<Func<TransportMessage, CancellationToken, Task>, CancellationToken>((h, _) =>
            {
                _handler = h;
            })
            .Returns(Task.CompletedTask);

        _runtime = new WorkersProcessingRuntime(
            _consumerMock.Object,
            _pipelineMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Message_processed_successfully_should_ack()
    {
        // Arrange
        var acked = new TaskCompletionSource(); 
        var ackMock = new Mock<IMessageAcknowledger>();
        ackMock.Setup(a => a.AckAsync())
            .Callback(() => acked.SetResult())
            .Returns(new ValueTask(Task.CompletedTask));
        
        var message = new TransportMessage
        {
            Acknowledger = ackMock.Object,
            Body = default,
            Headers = new Dictionary<string, object>()
            {
                {"entityType", "test-entity2"}
            },
            Queue = "test-queue"
        };

        await _runtime.StartAsync(CancellationToken.None);

        // Act
        await _handler(message, CancellationToken.None);
        await acked.Task;

        // Assert
        _pipelineMock.Verify(
            p => p.ExecuteAsync(It.IsAny<MessageContext>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);

        ackMock.Verify(a => a.AckAsync(), Times.Once);
        ackMock.Verify(a => a.NackAsync(It.IsAny<bool>()), Times.Never);
    }
    
    [Test]
    public async Task Pipeline_exception_should_nack_with_requeue()
    {
        // Arrange
        var nacked = new TaskCompletionSource();
        var ackMock = new Mock<IMessageAcknowledger>();
        ackMock.Setup(a => a.NackAsync(It.IsAny<bool>()))
            .Callback(() => nacked.SetResult())
            .Returns(new ValueTask(Task.CompletedTask));
        
        var message = new TransportMessage
        {
            Acknowledger = ackMock.Object,
            Body = default,
            Headers = new Dictionary<string, object>()
            {
                {"entityType", "test-entity"}
            },
            Queue = "test-queue"
        };

        _pipelineMock
            .Setup(p => p.ExecuteAsync(It.IsAny<MessageContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        await _runtime.StartAsync(CancellationToken.None);

        // Act
        await _handler(message, CancellationToken.None);
        await nacked.Task;

        // Assert
        ackMock.Verify(a => a.NackAsync(true), Times.Once);
        ackMock.Verify(a => a.AckAsync(), Times.Never);
    }
    
    [Test]
    public async Task WorkerId_should_be_present_in_context_items()
    {
        // Arrange
        MessageContext? capturedContext = null;
        
        var acked = new TaskCompletionSource(); 
        var ackMock = new Mock<IMessageAcknowledger>();
        ackMock.Setup(a => a.AckAsync())
            .Callback(() => acked.SetResult())
            .Returns(new ValueTask(Task.CompletedTask));

        _pipelineMock
            .Setup(p => p.ExecuteAsync(It.IsAny<MessageContext>(), It.IsAny<CancellationToken>()))
            .Callback<MessageContext, CancellationToken>((ctx, _) =>
            {
                capturedContext = ctx;
            })
            .Returns(Task.CompletedTask);

        var message = new TransportMessage
        {
            Acknowledger = ackMock.Object,
            Body = default,
            Headers = new Dictionary<string, object>()
            {
                {"entityType", "test-entity"}
            },
            Queue = "test-queue"
        };

        await _runtime.StartAsync(CancellationToken.None);

        // Act
        await _handler(message, CancellationToken.None);
        await acked.Task;

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext!.Items.Should().ContainKey("workerId");
    }
    
    [Test]
    public async Task StopAsync_should_cancel_workers()
    {
        // Arrange
        var ackMock = new Mock<IMessageAcknowledger>();
        var message = new TransportMessage
        {
            Acknowledger = ackMock.Object,
            Body = default,
            Headers = new Dictionary<string, object>()
            {
                {"entityType", "test-entity"}
            },
            Queue = "test-queue"
        };

        await _runtime.StartAsync(CancellationToken.None);

        // Act
        await _handler(message, CancellationToken.None);

        // Assert
        _pipelineMock.Verify(
            p => p.ExecuteAsync(It.IsAny<MessageContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}