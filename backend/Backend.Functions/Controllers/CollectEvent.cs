using System.Net;
using Backend.Functions.Models;
using Backend.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Functions.Controllers;

/// <summary>
/// HTTP-triggered function to receive client-side analytics events,
/// validate them, and enqueue them for processing.
/// </summary>
public class CollectEvent
{
    private readonly ILogger<CollectEvent> _logger;
    private readonly EventService _eventService;

    /// <summary>
    /// Initializes a new instance of <see cref="CollectEvent"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="eventService">Event service for parsing and validation.</param>
    public CollectEvent(ILogger<CollectEvent> logger, EventService eventService)
    {
        _logger = logger;
        _eventService = eventService;
    }

    /// <summary>
    /// Receives analytics events, validates them, and writes to queues.
    /// </summary>
    /// <param name="req">HTTP request containing JSON event payload.</param>
    /// <returns>Multi-output response with HTTP response and queue messages.</returns>
    [Function("CollectEvent")]
    public async Task<CollectEventResponse> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "events")] HttpRequestData req)
    {
        _logger.LogInformation("Received analytics event request");

        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var evt = _eventService.ParseEvent(body);

        if (evt == null || !_eventService.ValidateEvent(evt))
        {
            _logger.LogWarning("Invalid event payload received");
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid event payload");

            return new CollectEventResponse
            {
                HttpResponse = badResponse,
                MainQueueMessage = null,
                BackupQueueMessage = null
            };
        }

        var serializedEvent = _eventService.SerializeEvent(evt);
        _logger.LogInformation("Event validated and queued: {EventType}", evt.EventType);

        var okResponse = req.CreateResponse(HttpStatusCode.OK);
        await okResponse.WriteStringAsync("Event accepted");

        return new CollectEventResponse
        {
            HttpResponse = okResponse,
            MainQueueMessage = serializedEvent,
            BackupQueueMessage = serializedEvent
        };
    }
}