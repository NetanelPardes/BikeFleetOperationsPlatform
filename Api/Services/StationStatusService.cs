using System.Text.Json;
using Api.Models;
using StackExchange.Redis;

namespace Api.Services;

public class StationStatusService : IStationStatusService
{
    private readonly IDatabase _database;

    public StationStatusService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<StationStatus?> GetAsync(string stationId)
    {
        string key = $"station-status:{stationId}";

        RedisValue value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return null;
        }
        StationStatus? status = JsonSerializer.Deserialize<StationStatus>(value.ToString());
        return status;
    }
}