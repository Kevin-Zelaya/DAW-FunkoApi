using System.Collections.Concurrent;

namespace MinimalApi.Services.Email;

public class EmailQueueService
{
    private readonly ConcurrentQueue<EmailMessage> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public int Count => _queue.Count;

    public void Enqueue(EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _queue.Enqueue(message);
        _signal.Release();
    }

    public async Task<EmailMessage?> DequeueAsync(
        CancellationToken cancellationToken = default)
    {
        await _signal.WaitAsync(cancellationToken);

        return _queue.TryDequeue(out var message)
            ? message
            : null;
    }
}