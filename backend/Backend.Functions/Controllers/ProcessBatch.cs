using Backend.Functions.Models;
using Backend.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Backend.Functions.Controllers;

/// <summary>
/// Queue-triggered function to process analytics events,
/// enrich them with geolocation, and store them in Table Storage.
/// </summary>
public class ProcessBatch
{
    private readonly ILogger<ProcessBatch> _logger;
    private readonly StorageService _storageService;
    private readonly EventService _eventService;
    private readonly EnrichmentService _enrichmentService;

    /// <summary>
    /// Initializes a new instance of <see cref="ProcessBatch"/>.
    /// </summary>
    public ProcessBatch(
        ILogger<ProcessBatch> logger,
        EventService eventService,
        EnrichmentService enrichmentService,
        StorageService storageService)
    {
        _logger = logger;
        _eventService = eventService;
        _enrichmentService = enrichmentService;
        _storageService = storageService;
    }

    /// <summary>
    /// Processes a single message from the main queue.
    /// Parses, validates, enriches, and stores the event.
    /// Failed messages are moved to poison queue automatically by the runtime.
    /// </summary>
    /// <param name="message">Serialized event from mainqueue.</param>
    [Function("ProcessBatch")]
    public async Task Run(
        [QueueTrigger("mainqueue")] string message)
    {
        _logger.LogInformation("Processing queue message");

        var evt = _eventService.ParseEvent(message);
        if (evt == null || !_eventService.ValidateEvent(evt))
        {
            _logger.LogWarning("Invalid message format, will be moved to poison queue");
            throw new InvalidOperationException("Invalid event payload in queue message");
        }

        evt.BatchId = Guid.NewGuid().ToString();
        evt.ProcessedTimestamp = DateTime.UtcNow;

        await _enrichmentService.EnrichEventAsync(evt);
        await _storageService.SaveEventAsync(evt);

        _logger.LogInformation(
            "Event processed successfully: {EventType} for user {UserId}",
            evt.EventType,
            evt.UserId);
    }
}