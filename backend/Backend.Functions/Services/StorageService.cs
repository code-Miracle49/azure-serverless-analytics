using ASCTableStorage.Data;
using Backend.Functions.Models;

namespace Backend.Functions.Services;

/// <summary>
/// Handles saving EventModel entities to Azure Table Storage using ASCDataAccessLibrary.
/// </summary>
public class StorageService
{
    private readonly DataAccess<EventModel> _dataAccess;
    private readonly EventService _eventService;

    /// <summary>
    /// Initializes a new instance of <see cref="StorageService"/>.
    /// </summary>
    /// <param name="accountName">Azure Storage account name.</param>
    /// <param name="accountKey">Azure Storage account key.</param>
    public StorageService(string accountName, string accountKey)
    {
        _dataAccess = new DataAccess<EventModel>(accountName, accountKey);
        _eventService = new EventService();
    }

    /// <summary>
    /// Saves a single event to Table Storage.
    /// </summary>
    /// <param name="evt">EventModel to save.</param>
    public async Task SaveEventAsync(EventModel evt)
    {
        _eventService.PrepareForStorage(evt);
        await _dataAccess.ManageDataAsync(evt);
    }
}