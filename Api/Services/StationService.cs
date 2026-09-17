using Api.Data;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class StationService : IStationService
{
    private readonly IStationHistoryService _stationHistoryService;
    private readonly BikeFleetDbContext _dbContext;
    private readonly IStationStatusService _stationStatusService;

    public StationService(BikeFleetDbContext dbContext,IStationStatusService stationStatusService,IStationHistoryService stationHistoryService)
    {
        _dbContext = dbContext;
        _stationStatusService = stationStatusService;
        _stationHistoryService = stationHistoryService;
    }

    public async Task<List<StationResponseDto>> GetStationsAsync(StationFilterDto filters ,CancellationToken cancellationToken = default)
    {
        List<Station> stations = await _dbContext.Stations
                .ToListAsync(cancellationToken);

        List<StationResponseDto> result = new();

        foreach (Station station in stations)
        {
            StationStatus? stationStatus = await _stationStatusService.GetAsync(station.StationId);
            StationResponseDto response = CreateResponse(station, stationStatus);
            bool matches = MatchesFilters(response, filters);
            if (matches)
            {
                result.Add(response);
            }
        }
        return result;
    }

    public async Task<StationResponseDto?> GetStationByIdAsync(string id,CancellationToken cancellationToken = default)
    {
        Station? station = await _dbContext.Stations
            .FirstOrDefaultAsync(s => s.StationId == id , cancellationToken);
        if(station == null)
        {
            return null;
        }
        StationResponseDto result = new();
        StationStatus? stationStatus = await _stationStatusService.GetAsync(station.StationId);
        StationResponseDto response = CreateResponse(station, stationStatus);
        return response;
    }

    public async Task<StationStatusResponseDto?> GetStatusByIdAsync(string id,CancellationToken cancellationToken = default)
    {
        Station? station = await _dbContext.Stations
            .FirstOrDefaultAsync(s => s.StationId == id , cancellationToken);
        if(station == null)
        {
            return null;
        }
        StationStatus? stationStatus = await _stationStatusService.GetAsync(station.StationId);
        if(stationStatus == null)
        {
            return null;
        }
        StationStatusResponseDto stationStatusResponse = new()
        {
            StationId = station.StationId,
            AvailableBikes = stationStatus.AvailableVehicles,
            AvailableDocks = stationStatus.AvailableDocks,
            IsRenting = ConvertToBoolean(stationStatus.IsRenting),
            IsReturning = ConvertToBoolean(stationStatus.IsReturning),
            Status = CalculateStatus(station, stationStatus),
            LastReported = stationStatus.LastReported
        };
        return stationStatusResponse;

    }

    public async Task<List<StationHistoryResponseDto>?>GetStationHistoryAsync(string id,DateTime? from,DateTime? to,int limit,CancellationToken cancellationToken = default)
    {
        Station? station = await _dbContext.Stations
            .FirstOrDefaultAsync(station => station.StationId == id,cancellationToken);

        if (station is null)
        {
            return null;
        }

        List<StationStatusHistory> history = await _stationHistoryService.GetHistoryAsync(id,from,to,limit,cancellationToken);

        List<StationHistoryResponseDto> result = new();

        foreach (StationStatusHistory item in history)
        {
            StationStatus stationStatus = new()
            {
                StationId = item.StationId,
                AvailableVehicles = item.AvailableVehicles,
                AvailableDocks = item.AvailableDocks,
                IsRenting = item.IsRenting,
                IsReturning = item.IsReturning,
                LastReported = item.LastReported
            };

            StationHistoryResponseDto response = new()
            {
                Timestamp = item.RecordedAt,
                AvailableBikes = item.AvailableVehicles,
                AvailableDocks = item.AvailableDocks,
                Status = CalculateStatus(station,stationStatus)
            };

            result.Add(response);
        }

        return result;
    }

    private static StationResponseDto CreateResponse(Station station,StationStatus? stationStatus)
    {
        StationResponseDto response = new();

        response.Id = station.StationId;
        response.Name = station.Name;
        response.Latitude = station.Latitude;
        response.Longitude = station.Longitude;
        response.Capacity = station.Capacity;

        if (stationStatus is not null)
        {
            
            response.AvailableBikes = stationStatus.AvailableVehicles;

            response.AvailableDocks = stationStatus.AvailableDocks;

            response.IsRenting = ConvertToBoolean(stationStatus.IsRenting);

            response.IsReturning = ConvertToBoolean(stationStatus.IsReturning);
            
            response.LastReported = stationStatus.LastReported;
        }

        response.Status =CalculateStatus(station, stationStatus);

        return response;
    }

    private static bool MatchesFilters(StationResponseDto station,StationFilterDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            bool sameStatus = string.Equals(station.Status,filters.Status,StringComparison.OrdinalIgnoreCase);

            if (!sameStatus)
            {
                return false;
            }
        }

        if (filters.MinAvailableBikes.HasValue)
        {
            if (!station.AvailableBikes.HasValue)
            {
                return false;
            }

            if (station.AvailableBikes.Value < filters.MinAvailableBikes.Value)
            {
                return false;
            }
        }

        if (filters.IsRenting.HasValue)
        {
            if (station.IsRenting != filters.IsRenting.Value)
            {
                return false;
            }
        }

        if (filters.IsReturning.HasValue)
        {
            if (station.IsReturning != filters.IsReturning.Value)
            {
                return false;
            }
        }

        return true;
    }

    private static string CalculateStatus(Station station,StationStatus? stationStatus)
    {
        if (stationStatus is null)
        {
            return "unknown";
        }

        if (stationStatus.IsRenting != 1 ||stationStatus.IsReturning != 1)
        {
            return "out-of-service";
        }

        if (stationStatus.AvailableVehicles == 0)
        {
            return "empty";
        }

        if (stationStatus.AvailableDocks == 0)
        {
            return "full";
        }

        if (station.Capacity.HasValue && station.Capacity.Value > 0 && stationStatus.AvailableVehicles.HasValue)
        {
            double availableBikes =stationStatus.AvailableVehicles.Value;

            double capacity =station.Capacity.Value;

            double percentage = availableBikes / capacity;

            if (percentage <= 0.20)
            {
                return "low-availability";
            }
        }

        return "available";
    }

    private static bool? ConvertToBoolean(int? value)
    {
        if (value == 1)
        {
            return true;
        }

        if (value == 0)
        {
            return false;
        }

        return null;
    }
}