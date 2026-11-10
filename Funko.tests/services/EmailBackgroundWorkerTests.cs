
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using MinimalApi.Services.Email;

[TestFixture]
public class EmailBackgroundWorkerTests
{
    private Mock<IEmailQueueService> _queueMock = null!;
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
    private Mock<IServiceScope> _scopeMock = null!;
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private Mock<ILogger<EmailBackgroundWorker>> _loggerMock = null!;

    private EmailBackgroundWorker _worker = null!;

    [SetUp]
    public void Setup()
    {
        _queueMock = new Mock<IEmailQueueService>();
        _emailServiceMock = new Mock<IEmailService>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<EmailBackgroundWorker>>();

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEmailService)))
            .Returns(_emailServiceMock.Object);

        _scopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_scopeMock.Object);

        _worker = new EmailBackgroundWorker(
            _queueMock.Object,
            _scopeFactoryMock.Object,
            _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _worker?.Dispose();
    }

    private EmailMessage CreateEmail()
    {
        return new EmailMessage
        {
            To = "test@gmail.com",
            Subject = "Test",
            Body = "Contenido"
        };
    }

    private void SetupQueueWithEmailThenCancel(
        EmailMessage email,
        CancellationTokenSource cts)
    {
        _queueMock
            .SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(email)
            .Returns(async () =>
            {
                await cts.CancelAsync();
                return (EmailMessage?)null;
            });
    }

    [Test]
    public async Task ExecuteAsync_ValidEmail_SendsEmail()
    {
        // Arrange
        var email = CreateEmail();

        using var cts = new CancellationTokenSource();

        SetupQueueWithEmailThenCancel(email, cts);

        // Act
        await _worker.StartAsync(cts.Token);
        await _worker.ExecuteTask!;

        // Assert
        _emailServiceMock.Verify(
            x => x.SendAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);

        _scopeFactoryMock.Verify(
            x => x.CreateScope(),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_NullMessage_ContinuesProcessing()
    {
        // Arrange
        var email = CreateEmail();

        using var cts = new CancellationTokenSource();

        _queueMock
            .SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailMessage?)null)
            .ReturnsAsync(email)
            .Returns(async () =>
            {
                await cts.CancelAsync();
                return (EmailMessage?)null;
            });

        // Act
        await _worker.StartAsync(cts.Token);
        await _worker.ExecuteTask!;

        // Assert
        _queueMock.Verify(
            x => x.DequeueAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        _emailServiceMock.Verify(
            x => x.SendAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_CancellationRequested_StopsWorker()
    {
        // Arrange
        var dequeueStarted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _queueMock
            .Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken token) =>
            {
                dequeueStarted.TrySetResult(true);

                await Task.Delay(Timeout.Infinite, token);

                return (EmailMessage?)null;
            });

        // Act
        await _worker.StartAsync(CancellationToken.None);

        // Esperamos a que el worker entre en DequeueAsync
        await dequeueStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Detenemos el worker
        await _worker.StopAsync(CancellationToken.None);

        // Assert
        _queueMock.Verify(
            x => x.DequeueAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_EmailServiceThrows_ContinuesProcessing()
    {
        // Arrange
        var email = CreateEmail();

        using var cts = new CancellationTokenSource();

        SetupQueueWithEmailThenCancel(email, cts);

        _emailServiceMock
            .Setup(x => x.SendAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error enviando correo"));

        // Act
        await _worker.StartAsync(cts.Token);
        await _worker.ExecuteTask!;

        // Assert
        _emailServiceMock.Verify(
            x => x.SendAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);

        _queueMock.Verify(
            x => x.DequeueAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _scopeMock.Verify(
            x => x.Dispose(),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_QueueThrows_ContinuesProcessing()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        _queueMock
            .SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error en la cola"))
            .Returns(async () =>
            {
                await cts.CancelAsync();
                return (EmailMessage?)null;
            });

        // Act
        await _worker.StartAsync(cts.Token);
        await _worker.ExecuteTask!;

        // Assert
        _queueMock.Verify(
            x => x.DequeueAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _emailServiceMock.Verify(
            x => x.SendAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ScopeFactoryThrows_HandlesException()
    {
        // Arrange
        var email = CreateEmail();

        using var cts = new CancellationTokenSource();

        SetupQueueWithEmailThenCancel(email, cts);

        _scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Throws(new Exception("Error creando el scope"));

        // Act
        await _worker.StartAsync(cts.Token);
        await _worker.ExecuteTask!;

        // Assert
        _scopeFactoryMock.Verify(
            x => x.CreateScope(),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_EmailServiceNotRegistered_HandlesException()
    {
        // Arrange
        var email = CreateEmail();

        using var cts = new CancellationTokenSource();

        SetupQueueWithEmailThenCancel(email, cts);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IEmailService)))
            .Returns((object?)null);

        // Act
        await _worker.StartAsync(cts.Token);
        await _worker.ExecuteTask!;

        // Assert
        _serviceProviderMock.Verify(
            x => x.GetService(typeof(IEmailService)),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendAsync(
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_EmailProcessed_DisposesScope()
    {
        // Arrange
        var email = CreateEmail();

        using var cts = new CancellationTokenSource();

        SetupQueueWithEmailThenCancel(email, cts);

        // Act
        await _worker.StartAsync(cts.Token);
        await _worker.ExecuteTask!;

        // Assert
        _emailServiceMock.Verify(
            x => x.SendAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);

        _scopeMock.Verify(
            x => x.Dispose(),
            Times.Once);
    }
}
