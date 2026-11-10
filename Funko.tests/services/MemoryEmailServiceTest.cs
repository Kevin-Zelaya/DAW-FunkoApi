
using FluentAssertions;
using NUnit.Framework;
using MinimalApi.Services.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

[TestFixture]
public class MemoryEmailServiceTests
{
    private MemoryEmailService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new MemoryEmailService(
            NullLogger<MemoryEmailService>.Instance
        );

        MemoryEmailService.Clear();
    }

    /// <summary>
    /// Enviar un correo
    /// </summary>
    [Test]
    public async Task SendAsync_ValidEmail_ReturnsLastEmail()
    {
        // Arrange
        var email = new EmailMessage
        {
            To = "test@gmail.com",
            Subject = "Test",
            Body = "Contenido del correo",
            IsHtml = false
        };

        // Act
        await _service.SendAsync(email);

        var response = MemoryEmailService.GetLastEmail(email.To);

        // Assert
        response.Should().NotBeNull();
        response!.To.Should().Be(email.To);
        response.Subject.Should().Be(email.Subject);
        response.Body.Should().Be(email.Body);
    }

    /// <summary>
    /// Enviar varios correos
    /// </summary>
    [Test]
    public async Task SendBatchAsync_ValidEmails_ReturnsLastEmail()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new()
            {
                To = "test1@gmail.com",
                Subject = "Correo 1",
                Body = "Contenido 1"
            },
            new()
            {
                To = "test2@gmail.com",
                Subject = "Correo 2",
                Body = "Contenido 2"
            }
        };

        // Act
        await _service.SendBatchAsync(emails);

        var response = MemoryEmailService.GetLastEmail("test2@gmail.com");

        // Assert
        response.Should().NotBeNull();
        response!.To.Should().Be("test2@gmail.com");
        response.Subject.Should().Be("Correo 2");
    }

    /// <summary>
    /// Limpiar los correos almacenados
    /// </summary>
    [Test]
    public async Task Clear_EmailsExist_ReturnsNullLastEmail()
    {
        // Arrange
        var email = new EmailMessage
        {
            To = "test@gmail.com",
            Subject = "Test",
            Body = "Contenido"
        };

        await _service.SendAsync(email);

        // Act
        MemoryEmailService.Clear();

        var response = MemoryEmailService.GetLastEmail(email.To);

        // Assert
        response.Should().BeNull();
    }

    /// <summary>
    /// Obtener el último correo enviado
    /// </summary>
    [Test]
    public async Task GetLastEmail_EmailsExist_ReturnsLastEmail()
    {
        // Arrange
        var firstEmail = new EmailMessage
        {
            To = "test1@gmail.com",
            Subject = "Correo 1",
            Body = "Contenido 1"
        };

        var lastEmail = new EmailMessage
        {
            To = "test2@gmail.com",
            Subject = "Correo 2",
            Body = "Contenido 2"
        };

        await _service.SendAsync(firstEmail);
        await _service.SendAsync(lastEmail);

        // Act
        var response = MemoryEmailService.GetLastEmail(lastEmail.To);

        // Assert
        response.Should().NotBeNull();
        response!.To.Should().Be(lastEmail.To);
        response.Subject.Should().Be(lastEmail.Subject);
    }
}
