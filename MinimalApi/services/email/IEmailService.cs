
namespace MinimalApi.Services.Email;

/// <summary>
/// Contrato del servicio de email.
/// </summary>
/// <summary>
/// Contrato del servicio de email.
/// </summary>
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);

    Task SendHtmlEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

    Task SendBatchAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default);

    Task<EmailTemplate?> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default);
}