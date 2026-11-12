
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace MinimalApi.Services.Email;

public class MailKitEmailService(
    IOptions<SmtpSettings> options,
    ILogger<MailKitEmailService> _logger) : IEmailService
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var mimeMessage = new MimeMessage();

        // Remitente
        mimeMessage.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail));

        // Destinatario principal
        mimeMessage.To.Add(
            MailboxAddress.Parse(message.To));

        // Destinatarios en copia
        foreach (var cc in message.Cc)
            mimeMessage.Cc.Add(MailboxAddress.Parse(cc));

        // Destinatarios en copia oculta
        foreach (var bcc in message.Bcc)
            mimeMessage.Bcc.Add(MailboxAddress.Parse(bcc));

        // Responder a
        if (!string.IsNullOrWhiteSpace(message.ReplyTo))
            mimeMessage.ReplyTo.Add(
                MailboxAddress.Parse(message.ReplyTo));

        mimeMessage.Subject = message.Subject;

        // Construir el cuerpo y los adjuntos
        var bodyBuilder = new BodyBuilder();

        if (message.IsHtml)
            bodyBuilder.HtmlBody = message.Body;
        else
            bodyBuilder.TextBody = message.Body;

        foreach (var attachment in message.Attachments)
        {
            bodyBuilder.Attachments.Add(
                attachment.FileName,
                attachment.Content,
                ContentType.Parse(
                    attachment.ContentType ?? "application/octet-stream"));
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        _logger.LogInformation(
            "Enviando correo a {To}. Asunto: {Subject}",
            message.To,
            message.Subject);

        try
        {
            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                _settings.Host,
                _settings.Port,
                GetSecureSocket(_settings.Security),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await smtpClient.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password!,
                    cancellationToken);
            }

            await smtpClient.SendAsync(mimeMessage, cancellationToken);

            await smtpClient.DisconnectAsync(
                true,
                cancellationToken);

            _logger.LogInformation(
                "Correo enviado correctamente a {To}.",
                message.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error enviando correo a {To}.",
                message.To);

            throw;
        }
    }

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

    public async Task SendBatchAsync(
        IEnumerable<EmailMessage> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
        {
            await SendAsync(message, cancellationToken);
        }
    }

    public Task<EmailTemplate?> GetTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<EmailTemplate?>(null);
    }

    private static SecureSocketOptions GetSecureSocket(string security)
    {
        return security.ToLowerInvariant() switch
        {
            "ssl" => SecureSocketOptions.SslOnConnect,
            "tls" => SecureSocketOptions.StartTls,
            "none" => SecureSocketOptions.None,
            _ => SecureSocketOptions.Auto
        };
    }
}
