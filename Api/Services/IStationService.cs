using Api.DTOs;

namespace Api.Services;

public interface IStationService
{
    Task<List<StationResponseDto>> GetStationsAsync(StationFilterDto filters,CancellationToken cancellationToken = default);
    Task<StationResponseDto?> GetStationByIdAsync(string id,CancellationToken cancellationToken = default);
    Task<StationStatusResponseDto?> GetStatusByIdAsync(string id,CancellationToken cancellationToken = default);
    Task<List<StationHistoryResponseDto>?> GetStationHistoryAsync(string id,DateTime? from,DateTime? to,int limit,CancellationToken cancellationToken = default);
}