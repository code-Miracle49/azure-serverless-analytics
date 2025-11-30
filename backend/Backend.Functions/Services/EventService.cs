using System.Text.Json;
using Backend.Functions.Models;

namespace Backend.Functions.Services;

/// <summary>
/// Handles event parsing, validation, and preparation for storage/queueing.
/// </summary>
public class EventService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Parses JSON from HTTP request body into an EventModel.
    /// </summary>
    /// <param name="json">Raw JSON string from client.</param>
    /// <returns>EventModel if valid, null otherwise.</returns>
    public EventModel? ParseEvent(string json)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<EventModel>(json, JsonOptions);
            if (evt == null)
            {
                return null;
            }

            evt.ServerTimestamp = DateTime.UtcNow;
            return evt;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates required fields of the event.
    /// </summary>
    /// <param name="evt">EventModel instance.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public bool ValidateEvent(EventModel evt)
    {
        return !string.IsNullOrEmpty(evt.EventType)
            && !string.IsNullOrEmpty(evt.UserId)
            && !string.IsNullOrEmpty(evt.SessionId);
    }

    /// <summary>
    /// Prepares an event for Table Storage by setting partition and row keys.
    /// </summary>
    /// <param name="evt">EventModel to prepare.</param>
    public void PrepareForStorage(EventModel evt)
    {
        evt.PartitionKey = evt.TimestampUtc.ToString("yyyyMMdd");
        evt.RowKey = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Serializes an event to JSON for queue storage.
    /// </summary>
    /// <param name="evt">EventModel to serialize.</param>
    /// <returns>JSON string representation.</returns>
    public string SerializeEvent(EventModel evt)
    {
        return JsonSerializer.Serialize(evt, JsonOptions);
    }
}