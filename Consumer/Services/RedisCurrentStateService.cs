using System.Text.Json;
using Consumer.Models;
using StackExchange.Redis;

namespace Consumer.Services;

public class RedisCurrentStateService : ICurrentStateService
{
    private readonly IDatabase _database;
    public RedisCurrentStateService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<StationStatus?> GetAsync(string stationId)
    {
        RedisValue value =await _database.StringGetAsync($"station-status:{stationId}");
        if (value.IsNullOrEmpty)
        {
            return null;
        }
        return JsonSerializer.Deserialize<StationStatus>(value.ToString());
    }

    public async Task SetAsync(StationStatus status)
    {
        if (string.IsNullOrWhiteSpace(status.StationId))
        {
            throw new ArgumentException("Station ID is missing.");
        }
        string json = JsonSerializer.Serialize(status);

        await _database.StringSetAsync($"station-status:{status.StationId}",json);
    }
}