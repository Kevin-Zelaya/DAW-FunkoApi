
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

[TestFixture]
public class GlobalExceptionHandlerTests
{
    private Mock<ILogger<GlobalExceptionHandler>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<GlobalExceptionHandler>>();
    }

    private async Task<(DefaultHttpContext Context, string Body)> ExecuteMiddleware(
        Exception? exception = null)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        context.Request.Path = "/api/users";
        context.Request.Method = "GET";

        RequestDelegate next = _ =>
        {
            if (exception != null)
                throw exception;

            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandler(
            next,
            _logger.Object);

        // Act
        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;

        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        return (context, body);
    }

    private static JsonDocument ParseResponse(string body)
    {
        return JsonDocument.Parse(body);
    }

    #region Excepciones HTTP

    [Test]
    public async Task ArgumentException_ReturnsBadRequest()
    {
        var (context, body) = await ExecuteMiddleware(
            new ArgumentException("Argumento incorrecto"));

        using var json = ParseResponse(body);
        var root = json.RootElement;

        Assert.That(context.Response.StatusCode, Is.EqualTo(400));
        Assert.That(root.GetProperty("status").GetInt32(), Is.EqualTo(400));
        Assert.That(
            root.GetProperty("message").GetString(),
            Is.EqualTo("Argumento incorrecto"));
    }

    [Test]
    public async Task UnauthorizedAccessException_ReturnsUnauthorized()
    {
        var (context, body) = await ExecuteMiddleware(
            new UnauthorizedAccessException());

        using var json = ParseResponse(body);
        var root = json.RootElement;

        Assert.That(context.Response.StatusCode, Is.EqualTo(401));
        Assert.That(
            root.GetProperty("message").GetString(),
            Is.EqualTo("No autorizado"));
    }

    [Test]
    public async Task TimeoutException_ReturnsRequestTimeout()
    {
        var (context, body) = await ExecuteMiddleware(
            new TimeoutException());

        using var json = ParseResponse(body);
        var root = json.RootElement;

        Assert.That(context.Response.StatusCode, Is.EqualTo(408));
        Assert.That(
            root.GetProperty("message").GetString(),
            Is.EqualTo("Tiempo de espera agotado"));
    }

    [Test]
    public async Task DbUpdateConcurrencyException_ReturnsConflict()
    {
        var (context, body) = await ExecuteMiddleware(
            new DbUpdateConcurrencyException());

        using var json = ParseResponse(body);
        var root = json.RootElement;

        Assert.That(context.Response.StatusCode, Is.EqualTo(409));
        Assert.That(
            root.GetProperty("message").GetString(),
            Is.EqualTo(
                "El recurso ha sido modificado por otro usuario. Recarga los datos e inténtalo de nuevo."));
    }

    [Test]
    public async Task Exception_ReturnsInternalServerError()
    {
        var (context, body) = await ExecuteMiddleware(
            new Exception("Error interno"));

        using var json = ParseResponse(body);
        var root = json.RootElement;

        Assert.That(context.Response.StatusCode, Is.EqualTo(500));
        Assert.That(
            root.GetProperty("message").GetString(),
            Is.EqualTo("Ha ocurrido un error interno"));
    }

    #endregion

    #region Respuesta JSON

    [Test]
    public async Task Exception_ResponseContainsErrorId()
    {
        var (_, body) = await ExecuteMiddleware(
            new Exception());

        using var json = ParseResponse(body);

        var errorId = json.RootElement
            .GetProperty("errorId")
            .GetString();

        Assert.That(errorId, Is.Not.Null);
        Assert.That(errorId, Has.Length.EqualTo(8));
    }

    [Test]
    public async Task Exception_ResponseContainsTimestamp()
    {
        var (_, body) = await ExecuteMiddleware(
            new Exception());

        using var json = ParseResponse(body);

        var timestamp = json.RootElement
            .GetProperty("timestamp")
            .GetString();

        Assert.That(timestamp, Is.Not.Null);
        Assert.That(
            DateTime.TryParse(timestamp, out _),
            Is.True);
    }

    [Test]
    public async Task Exception_ResponseHasJsonContentType()
    {
        var (context, _) = await ExecuteMiddleware(
            new Exception());

        Assert.That(
            context.Response.ContentType,
            Is.EqualTo("application/json"));
    }

    #endregion

    #region Logger

    [Test]
    public async Task Exception_LogsError()
    {
        await ExecuteMiddleware(new Exception("Error de prueba"));

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Excepción no manejada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Ejecución normal

    [Test]
    public async Task NoException_ContinuesNormally()
    {
        var (context, body) = await ExecuteMiddleware();

        Assert.That(context.Response.StatusCode, Is.EqualTo(200));
        Assert.That(body, Is.Empty);

        _logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion
}
