using Consumer.Models;

namespace Consumer.Services;

public interface ICurrentStateService
{
    Task<StationStatus?> GetAsync(string stationId);
    Task SetAsync(StationStatus status);
}