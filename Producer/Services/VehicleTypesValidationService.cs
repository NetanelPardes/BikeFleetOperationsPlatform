using Producer.DTOs;

namespace Producer.Services;

public class VehicleTypesValidationService
{
    public bool IsValid(VehicleTypeDto vehicleType)
    {
        return !string.IsNullOrWhiteSpace(vehicleType.VehicleTypeId)
        && !string.IsNullOrWhiteSpace(vehicleType.FormFactor)
        && !string.IsNullOrWhiteSpace(vehicleType.PropulsionType)
        && (vehicleType.MaxRangeMeters is null || vehicleType.MaxRangeMeters is >= 0);
    }
}
