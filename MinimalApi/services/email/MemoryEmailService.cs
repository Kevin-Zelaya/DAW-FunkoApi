using Microsoft.Extensions.Logging;
using Serilog;

namespace MinimalApi.Services.Email;

/// <summary>
/// Implementación de IEmailService para desarrollo que solo loguea los emails.
/// </summary>
public class MemoryEmailService : IEmailService
{
    private readonly ILogger<MemoryEmailService> _logger;

    public MemoryEmailService(ILogger<MemoryEmailService> logger)
    {
        _logger = logger;
    }

    public Task EnqueueEmailAsync(EmailMessage message)
    {
        LogEmail(message, "ENQUEUED");
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(EmailMessage message)
    {
        LogEmail(message, "SENT");
        return Task.CompletedTask;
    }

    private void LogEmail(EmailMessage message, string status)
    {
        Log.Information("=== EMAIL {Status} ===", status);
        Log.Information("Para: {To}", message.To);
        Log.Information("Asunto: {Subject}", message.Subject);
        Log.Information("Tipo: {Type}", message.IsHtml ? "HTML" : "Texto plano");
        Log.Debug("Cuerpo: {Body}", message.Body);
        Log.Information("======================");
    }
}