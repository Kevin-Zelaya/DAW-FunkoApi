namespace MinimalApi.Services.Email;

public interface IEmailQueueService
{
    int Count { get; }

    void Enqueue(EmailMessage message);

    Task<EmailMessage?> DequeueAsync(
        CancellationToken cancellationToken = default);
}