using Azure.Storage.Queues;

namespace Backend.Functions.Services;

/// <summary>
/// Service to enqueue events to Azure Storage Queues using the Azure SDK.
/// Use this for programmatic queue access outside of function bindings.
/// </summary>
public class QueueService
{
    private readonly QueueClient _mainQueue;
    private readonly QueueClient _backupQueue;
    private readonly QueueClient _poisonQueue;

    /// <summary>
    /// Initializes a new instance of <see cref="QueueService"/>.
    /// </summary>
    /// <param name="connectionString">Azure Storage connection string.</param>
    public QueueService(string connectionString)
    {
        var options = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };

        _mainQueue = new QueueClient(connectionString, "mainqueue", options);
        _backupQueue = new QueueClient(connectionString, "backupqueue", options);
        _poisonQueue = new QueueClient(connectionString, "poisonqueue", options);
    }

    /// <summary>
    /// Ensures all queues exist in Azure Storage.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _mainQueue.CreateIfNotExistsAsync();
        await _backupQueue.CreateIfNotExistsAsync();
        await _poisonQueue.CreateIfNotExistsAsync();
    }

    /// <summary>
    /// Adds event JSON to main and backup queues.
    /// </summary>
    /// <param name="eventJson">Serialized event JSON.</param>
    public async Task EnqueueAsync(string eventJson)
    {
        await Task.WhenAll(
            _mainQueue.SendMessageAsync(eventJson),
            _backupQueue.SendMessageAsync(eventJson));
    }

    /// <summary>
    /// Adds a failed message to the poison queue.
    /// </summary>
    /// <param name="message">Failed message.</param>
    public async Task SendToPoisonQueueAsync(string message)
    {
        await _poisonQueue.SendMessageAsync(message);
    }

    /// <summary>
    /// Adds failed messages to the poison queue.
    /// </summary>
    /// <param name="messages">Failed messages.</param>
    public async Task SendToPoisonQueueAsync(IEnumerable<string> messages)
    {
        var tasks = messages.Select(msg => _poisonQueue.SendMessageAsync(msg));
        await Task.WhenAll(tasks);
    }
}