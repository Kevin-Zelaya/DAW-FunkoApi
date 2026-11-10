using MinimalApi.Services.Email;

public interface IEmailQueueService
{
    int Count { get; }

    Task<EmailMessage?> DequeueAsync(CancellationToken cancellationToken = default);
    void Enqueue(EmailMessage message);
}
