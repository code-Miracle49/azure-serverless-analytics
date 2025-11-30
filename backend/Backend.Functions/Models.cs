using ASCTableStorage.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Backend.Functions.Models;

/// <summary>
/// Represents a single client-side analytics event sent from the frontend.
/// Extends TableEntityBase for Azure Table Storage compatibility.
/// </summary>
public class EventModel : TableEntityBase, ITableExtra
{
    /// <summary>
    /// The type/category of the event (e.g., "page_view", "button_click").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The exact timestamp the event occurred on the client (UTC).
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// The user identifier (anonymous ID or auth ID).
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The session identifier for grouping events.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The full URL where the event occurred.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The referring URL that led to this page.
    /// </summary>
    public string? Referrer { get; set; }

    /// <summary>
    /// Browser name and version (e.g., "Chrome 120").
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// Device type (e.g., "Desktop", "Mobile", "Tablet").
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// Screen dimensions (e.g., "1920x1080").
    /// </summary>
    public string? ScreenSize { get; set; }

    /// <summary>
    /// Client IP address for geolocation.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Client latitude for geolocation enrichment.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Client longitude for geolocation enrichment.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Enriched city from reverse geocoding.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Enriched country/region from reverse geocoding.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Server timestamp when the event was received.
    /// </summary>
    public DateTime ServerTimestamp { get; set; }

    /// <summary>
    /// Batch identifier for grouping processed events.
    /// </summary>
    public string? BatchId { get; set; }

    /// <summary>
    /// Timestamp when the event was processed by the batch processor.
    /// </summary>
    public DateTime? ProcessedTimestamp { get; set; }

    /// <summary>
    /// Event-specific properties serialized as JSON.
    /// </summary>
    public string? PropertiesJson { get; set; }

    /// <summary>
    /// Table name for Azure Table Storage.
    /// </summary>
    public string TableReference => "AnalyticsEvents";

    /// <summary>
    /// Returns the unique identifier for this entity.
    /// </summary>
    public string GetIDValue() => RowKey ?? string.Empty;
}

/// <summary>
/// Partial model for Azure Maps reverse geocoding response.
/// </summary>
public class AzureMapsResponse
{
    public AzureMapsAddress[] Addresses { get; set; } = Array.Empty<AzureMapsAddress>();
}

/// <summary>
/// Address wrapper from Azure Maps response.
/// </summary>
public class AzureMapsAddress
{
    public Address Address { get; set; } = new();
}

/// <summary>
/// Address details from Azure Maps reverse geocoding.
/// </summary>
public class Address
{
    public string Locality { get; set; } = string.Empty;
    public string CountrySubdivision { get; set; } = string.Empty;
}

/// <summary>
/// Response model for the CollectEvent function with queue output.
/// </summary>
public class CollectEventResponse
{
    /// <summary>
    /// HTTP response to return to the client.
    /// </summary>
    public required HttpResponseData HttpResponse { get; set; }

    /// <summary>
    /// Message to send to the main queue.
    /// </summary>
    [QueueOutput("mainqueue")]
    public string? MainQueueMessage { get; set; }

    /// <summary>
    /// Message to send to the backup queue.
    /// </summary>
    [QueueOutput("backupqueue")]
    public string? BackupQueueMessage { get; set; }
}

/// <summary>
/// Response model for the ProcessBatch function with poison queue output.
/// </summary>
public class ProcessBatchResponse
{
    /// <summary>
    /// Messages to send to the poison queue for failed processing.
    /// </summary>
    [QueueOutput("poisonqueue")]
    public string[]? PoisonQueueMessages { get; set; }
}