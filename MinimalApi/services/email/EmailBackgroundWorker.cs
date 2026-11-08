using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MinimalApi.Services.Email;

public class EmailBackgroundWorker : BackgroundService
{
    private readonly EmailQueueService _emailQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailBackgroundWorker> _logger;

    public EmailBackgroundWorker(
        EmailQueueService emailQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundWorker> logger)
    {
        _emailQueue = emailQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _emailQueue.DequeueAsync(stoppingToken);

                if (message is null)
                    continue;

                using var scope = _scopeFactory.CreateScope();

                var emailService = scope.ServiceProvider
                    .GetRequiredService<IEmailService>();

                await emailService.SendAsync(message, stoppingToken);

                _logger.LogInformation(
                    "Correo enviado correctamente a {To}.",
                    message.To);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando un correo de la cola.");
            }
        }

        _logger.LogInformation("EmailBackgroundWorker detenido.");
    }
}