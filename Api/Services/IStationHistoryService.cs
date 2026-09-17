using Api.Models;

namespace Api.Services;

public interface IStationHistoryService
{
    Task<List<StationStatusHistory>> GetHistoryAsync(string stationId,DateTime? from,DateTime? to,int limit,CancellationToken cancellationToken = default);
}