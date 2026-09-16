using System.Text.Json;
using Consumer.Data;
using Consumer.Entities;
using Consumer.Models;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Services;

public class StationInformationHandler : IStationInformationHandler
{
    private readonly BikeFleetDbContext _dbContext;

    public StationInformationHandler(BikeFleetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(StationInformation station, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(station.StationId))
        {
            
        }
    }
}