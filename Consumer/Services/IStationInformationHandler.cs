using Consumer.Models;

namespace Consumer.Services;

public interface IStationInformationHandler
{
    Task HandleAsync(StationInformation station, CancellationToken cancellationToken = default);
}