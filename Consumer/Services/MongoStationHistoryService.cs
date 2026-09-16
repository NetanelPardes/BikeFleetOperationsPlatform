using Consumer.Models;
using MongoDB.Driver;

namespace Consumer.Services;

public class MongoStationHistoryService: IStationHistoryService
{
    private readonly IMongoCollection<StationStatusHistory>_collection;
    public MongoStationHistoryService()
    {
        string connectionString = GetRequiredEnvironmentVariable("MONGODB_CONNECTION_STRING");
        string databaseName = GetRequiredEnvironmentVariable("MONGODB_DATABASE");
        string collectionName =GetRequiredEnvironmentVariable("MONGODB_HISTORY_COLLECTION");
        MongoClient client = new(connectionString);
        IMongoDatabase database =client.GetDatabase(databaseName);
        _collection = database.GetCollection<StationStatusHistory>(collectionName);
    }

    public async Task SaveAsync(StationStatus status,CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status.StationId))
        {
            throw new ArgumentException("Station ID is missing.");
        }

        StationStatusHistory history = new()
        {
            StationId = status.StationId,
            AvailableVehicles = status.AvailableVehicles,
            AvailableDocks = status.AvailableDocks,
            IsRenting = status.IsRenting,
            IsReturning = status.IsReturning,
            LastReported = status.LastReported,
            RecordedAt = DateTime.UtcNow
        };
        await _collection.InsertOneAsync(history,cancellationToken: cancellationToken);
    }
    private static string GetRequiredEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name)?? throw new InvalidOperationException($"{name} is missing from .env");
    }
}