using Api.DTOs;
using Api.Data;


namespace Api.Services;

public class DashboardService : IDashboardService
{
    private readonly BikeFleetDbContext _dbContext;
    private readonly IStationStatusService _stationStatusService;
    private readonly IStationService _stationService;
    public DashboardService(BikeFleetDbContext dbContext,IStationStatusService stationStatusService,IStationService stationService)
    {
        _dbContext = dbContext;
        _stationStatusService = stationStatusService;
        _stationService = stationService;
    }

    public async Task<DashboardResponseDto> GetDashboard(CancellationToken cancellationToken = default)
    {
        List<StationResponseDto> stationList = await _stationService.GetStationsAsync(new StationFilterDto(),cancellationToken);
       
        DashboardResponseDto dashboardResponse = new()
        {
            TotalStations = stationList.Count,

            EmptyStations = stationList.Count(station => station.Status == "empty"),

            FullStations = stationList.Count(station => station.Status == "full"),

            LowAvailabilityStations = stationList.Count(station => station.Status == "low-availability"),

            OutOfServiceStations = stationList.Count(station => station.Status == "out-of-service"),
            
            AvailableStations = stationList.Count(station => station.Status == "available"),

            UnknownStations = stationList.Count(station => station.Status == "unknown"),

            LastUpdated =
                stationList.Max(station => station.LastReported)
                    is long lastReported
                        ? DateTimeOffset.FromUnixTimeSeconds(lastReported)
                        : null
        };

        return dashboardResponse;
    }
}