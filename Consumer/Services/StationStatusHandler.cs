using Consumer.Data;
using Consumer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consumer.Services;

public class StationStatusHandler: IStationStatusHandler
{
    private readonly ILogger<StationStatusHandler> _logger;
    private readonly BikeFleetDbContext _dbContext;
    private readonly ICurrentStateService _currentStateService;
    private readonly IStationHistoryService _historyService;

    public StationStatusHandler(BikeFleetDbContext dbContext,ICurrentStateService currentStateService,IStationHistoryService historyService,ILogger<StationStatusHandler> logger)
    {
        _dbContext = dbContext;
        _currentStateService = currentStateService;
        _historyService = historyService;
        _logger = logger;
    }

    public async Task HandleAsync(StationStatus status,CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status.StationId))
        {
            throw new ArgumentException("Station ID is missing.");
        }

        bool stationExists =await _dbContext.Stations.AnyAsync(station => station.StationId == status.StationId,cancellationToken);

        if (!stationExists)
        {
            //_logger.LogWarning("Status ignored because station {StationId} does not exist.",status.StationId);
            return;
        }

        StationStatus? lastStatus =await _currentStateService.GetAsync(status.StationId);

        if (lastStatus is not null &&HasSameOperationalState(lastStatus, status))
        {
            //_logger.LogDebug("Status unchanged for station {StationId}.",status.StationId);
            return;
        }

        await _historyService.SaveAsync(status,cancellationToken);
        _logger.LogInformation("Station {StationId} status saved to MongoDB history.",status.StationId);

        await _currentStateService.SetAsync(status);
        _logger.LogInformation("Station {StationId} current status saved to Redis.",status.StationId);
    }

    private static bool HasSameOperationalState(StationStatus previous,StationStatus current)
    {
        return previous.AvailableVehicles == current.AvailableVehicles
            && previous.AvailableDocks == current.AvailableDocks
            && previous.IsRenting == current.IsRenting
            && previous.IsReturning == current.IsReturning;
    }
}