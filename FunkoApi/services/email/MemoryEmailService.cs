
using System.Collections.Concurrent;
using Serilog;

namespace MinimalApi.Services.Email;

/// <summary>
/// Servicio de email que simula el envío y almacena
/// los mensajes en memoria.
/// </summary>
public class MemoryEmailService : IEmailService
{
    private readonly ILogger<MemoryEmailService> _logger;

    public static ConcurrentBag<EmailMessage> SentEmails { get; }
        = new();

    public MemoryEmailService(
        ILogger<MemoryEmailService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Envía y almacena un correo.
    /// </summary>
    
    public Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SentEmails.Add(message);

        Console.WriteLine();
        Console.WriteLine("========== EMAIL ==========");
        Console.WriteLine($"Para:   {message.To}");
        Console.WriteLine($"Asunto: {message.Subject}");

        // Eliminar HTML y espacios innecesarios
        var body = System.Text.RegularExpressions.Regex.Replace(
            message.Body, "<[^>]*>", " ");

        body = System.Net.WebUtility.HtmlDecode(body);

        body = System.Text.RegularExpressions.Regex.Replace(
            body, @"\s+", " ").Trim();

        Console.WriteLine($"Mensaje: {body}");
        Console.WriteLine("===========================");
        Console.WriteLine();

        return Task.CompletedTask;
    }


    /// <summary>
    /// Envía un correo HTML.
    /// </summary>
    public Task SendHtmlEmailAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            To = to,
            Subject = subject,
            Body = htmlBody,
            IsHtml = true
        };

        return SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Envía varios correos.
    /// </summary>
    public async Task SendBatchAsync(
        IEnumerable<EmailMessage> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
        {
            await SendAsync(message, cancellationToken);
        }
    }

    /// <summary>
    /// Recupera una plantilla por su nombre.
    /// </summary>
    public Task<EmailTemplate?> GetTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // No utilizamos plantillas almacenadas en archivos todavía.
        return Task.FromResult<EmailTemplate?>(null);
    }

    /// <summary>
    /// Limpia los correos almacenados.
    /// 
    /// </summary>
    public static void Clear()
    {
        while (SentEmails.TryTake(out _))
        {
        }
    }

    /// <summary>
    /// Recupera el último correo encontrado para un destinatario.
    /// </summary>
    public static EmailMessage? GetLastEmail(string to)
    {
        return SentEmails
            .LastOrDefault(email =>
                email.To.Equals(
                    to,
                    StringComparison.OrdinalIgnoreCase));
    }
}
