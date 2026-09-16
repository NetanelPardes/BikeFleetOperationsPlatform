using System.Text.Json;
using Consumer.Data;
using Consumer.Entities;
using Consumer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consumer.Services;

public class StationInformationHandler : IStationInformationHandler
{
    private readonly ILogger<StationInformationHandler> _logger;
    private readonly BikeFleetDbContext _dbContext;

    public StationInformationHandler(BikeFleetDbContext dbContext, ILogger<StationInformationHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(StationInformation station, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(station.StationId))
        {
            throw new ArgumentException("Station ID is missing.");
        }
        StationEntity? existingStation = await _dbContext.Stations.FirstOrDefaultAsync(s => s.StationId == station.StationId,cancellationToken);
        if(existingStation == null)
        {
            StationEntity newStation = new()
            {
                StationId = station.StationId,
                Name = station.Name,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                Capacity = station.Capacity,
                AdditionalFieldsJson = station.AdditionalFields is null? null: JsonSerializer.Serialize(station.AdditionalFields)
            };
            await _dbContext.Stations.AddAsync(newStation, cancellationToken);
        }
        else
        {
            existingStation.Name = station.Name;
            existingStation.Latitude = station.Latitude;
            existingStation.Longitude = station.Longitude;
            existingStation.Capacity = station.Capacity;
            existingStation.AdditionalFieldsJson =station.AdditionalFields is null? null: JsonSerializer.Serialize(station.AdditionalFields);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Station {StationId} saved to MySQL.",station.StationId);
    }
}