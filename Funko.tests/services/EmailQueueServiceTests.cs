using FluentAssertions;
using NUnit.Framework;
using MinimalApi.Services.Email;
[TestFixture]
public class EmailQueueServiceTests
{
    private EmailQueueService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new EmailQueueService();
    }

    /// <summary>
    /// Encolar un correo
    /// </summary>
    [Test]
    public async Task Enqueue_ValidEmail_DequeueReturnsSameEmail()
    {
        // Arrange
        var email = new EmailMessage
        {
            To = "test@gmail.com",
            Subject = "Test",
            Body = "Contenido del correo"
        };

        // Act
        _service.Enqueue(email);

        var response = await _service.DequeueAsync(
            CancellationToken.None
        );

        // Assert
        response.Should().NotBeNull();
        response!.To.Should().Be(email.To);
        response.Subject.Should().Be(email.Subject);
        response.Body.Should().Be(email.Body);
    }

    /// <summary>
    /// Desencolar varios correos respetando el orden
    /// </summary>
    [Test]
    public async Task DequeueAsync_MultipleEmails_ReturnsEmailsInOrder()
    {
        // Arrange
        var firstEmail = new EmailMessage
        {
            To = "test1@gmail.com",
            Subject = "Correo 1",
            Body = "Contenido 1"
        };

        var secondEmail = new EmailMessage
        {
            To = "test2@gmail.com",
            Subject = "Correo 2",
            Body = "Contenido 2"
        };

        _service.Enqueue(firstEmail);
        _service.Enqueue(secondEmail);

        // Act
        var firstResponse = await _service.DequeueAsync(
            CancellationToken.None
        );

        var secondResponse = await _service.DequeueAsync(
            CancellationToken.None
        );

        // Assert
        firstResponse.Should().NotBeNull();
        firstResponse!.To.Should().Be(firstEmail.To);

        secondResponse.Should().NotBeNull();
        secondResponse!.To.Should().Be(secondEmail.To);
    }
}