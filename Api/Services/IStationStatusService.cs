using Api.Models;

namespace Api.Services;

public interface IStationStatusService
{
    Task<StationStatus?> GetAsync(string stationId);
}