using System.Net.Http.Json;
using Backend.Functions.Models;

namespace Backend.Functions.Services;

/// <summary>
/// Service to enrich events with geolocation information using Azure Maps.
/// </summary>
public class EnrichmentService
{
    private readonly HttpClient _httpClient;
    private readonly string? _mapsKey;

    /// <summary>
    /// Initializes a new instance of <see cref="EnrichmentService"/>.
    /// </summary>
    public EnrichmentService()
    {
        _httpClient = new HttpClient();
        _mapsKey = Environment.GetEnvironmentVariable("AzureMapsKey");
    }

    /// <summary>
    /// Initializes a new instance of <see cref="EnrichmentService"/> with injected HttpClient.
    /// </summary>
    /// <param name="httpClient">HttpClient instance for making API calls.</param>
    public EnrichmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _mapsKey = Environment.GetEnvironmentVariable("AzureMapsKey");
    }

    /// <summary>
    /// Enriches an event with city and country based on latitude/longitude.
    /// </summary>
    /// <param name="evt">EventModel to enrich.</param>
    public async Task EnrichEventAsync(EventModel evt)
    {
        if (!evt.Latitude.HasValue || !evt.Longitude.HasValue || string.IsNullOrEmpty(_mapsKey))
        {
            return;
        }

        var url = $"https://atlas.microsoft.com/search/address/reverse/json" +
                  $"?subscription-key={_mapsKey}" +
                  $"&api-version=1.0" +
                  $"&query={evt.Latitude},{evt.Longitude}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<AzureMapsResponse>(url);
            if (response?.Addresses?.Length > 0)
            {
                var address = response.Addresses[0].Address;
                evt.City = address.Locality;
                evt.Country = address.CountrySubdivision;
            }
        }
        catch
        {
            // Ignore enrichment failures - non-critical operation
        }
    }
}