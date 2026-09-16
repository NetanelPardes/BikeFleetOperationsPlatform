using Consumer.Models;

namespace Consumer.Services;

public interface IStationHistoryService
{
    Task SaveAsync(StationStatus status,CancellationToken cancellationToken = default);
}