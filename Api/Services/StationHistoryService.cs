using Api.Models;
using MongoDB.Driver;

namespace Api.Services;

public class StationHistoryService : IStationHistoryService
{
    private readonly IMongoCollection<StationStatusHistory> _collection;

    public StationHistoryService(IMongoClient mongoClient)
    {
        string databaseName =Environment.GetEnvironmentVariable("MONGODB_DATABASE")?? throw new InvalidOperationException("MONGODB_DATABASE is missing from .env");

        string collectionName =Environment.GetEnvironmentVariable("MONGODB_HISTORY_COLLECTION")?? throw new InvalidOperationException("MONGODB_HISTORY_COLLECTION is missing from .env");

        IMongoDatabase database = mongoClient.GetDatabase(databaseName);

        _collection = database.GetCollection<StationStatusHistory>(collectionName);
    }

    public async Task<List<StationStatusHistory>> GetHistoryAsync(string stationId,DateTime? from,DateTime? to,int limit,CancellationToken cancellationToken = default)
    {
        FilterDefinition<StationStatusHistory> filter = Builders<StationStatusHistory>.Filter.Eq(history => history.StationId,stationId);

        if (from.HasValue)
        {
            filter &= Builders<StationStatusHistory>.Filter.Gte(history => history.RecordedAt,from.Value.ToUniversalTime());
        }

        if (to.HasValue)
        {
            filter &= Builders<StationStatusHistory>.Filter.Lte(history => history.RecordedAt,to.Value.ToUniversalTime());
        }

        List<StationStatusHistory> history = await _collection
                .Find(filter)
                .SortByDescending(item => item.RecordedAt)
                .Limit(limit)
                .ToListAsync(cancellationToken);

        return history;
    }
}