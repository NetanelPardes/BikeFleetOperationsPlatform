using Producer.DTOs;

namespace Producer.Services;

public class StationStatusValidationService
{
    public bool IsValid(StationStatusDto status)
    {
        return !string.IsNullOrWhiteSpace(status.StationId)
        && status.AvailableVehicles is >= 0
        && status.AvailableDocks is >= 0
        && status.IsRenting is 0 or 1
        && status.IsReturning is 0 or 1;
    }
}