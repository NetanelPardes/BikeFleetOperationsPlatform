using Consumer.Models;

namespace Consumer.Services;

public interface IStationStatusHandler
{
    Task HandleAsync(StationStatus status,CancellationToken cancellationToken = default);
}