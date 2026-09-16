using System.Text.Json;
using Consumer.Data;
using Consumer.Entities;
using Consumer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consumer.Services;

public class VehicleTypesHandler : IVehicleTypesHandler
{
    private readonly ILogger<VehicleTypesHandler> _logger;
    private readonly BikeFleetDbContext _dbContext;

    public VehicleTypesHandler(BikeFleetDbContext dbContext,ILogger<VehicleTypesHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(VehicleType vehicleType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vehicleType.VehicleTypeId))
        {
            throw new ArgumentException("Vehicle type ID is missing.");
        }
        VehicleTypeEntity? existingVehicleType =await _dbContext.VehicleTypes.FirstOrDefaultAsync(v => v.VehicleTypeId == vehicleType.VehicleTypeId, cancellationToken);
        if (existingVehicleType is null)
        {
            VehicleTypeEntity newVehicleType = new()
            {
                VehicleTypeId = vehicleType.VehicleTypeId,
                FormFactor = vehicleType.FormFactor,
                PropulsionType = vehicleType.PropulsionType,
                MaxRangeMeters = vehicleType.MaxRangeMeters,
                AdditionalFieldsJson =vehicleType.AdditionalFields is null? null: JsonSerializer.Serialize(vehicleType.AdditionalFields)
            };
            await _dbContext.VehicleTypes.AddAsync(newVehicleType,cancellationToken);
        }
        else
        {
            existingVehicleType.FormFactor =vehicleType.FormFactor;
            existingVehicleType.PropulsionType =vehicleType.PropulsionType;
            existingVehicleType.MaxRangeMeters =vehicleType.MaxRangeMeters;
            existingVehicleType.AdditionalFieldsJson =vehicleType.AdditionalFields is null? null: JsonSerializer.Serialize(vehicleType.AdditionalFields);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Vehicle type {VehicleTypeId} saved to MySQL.",vehicleType.VehicleTypeId);
    }
}