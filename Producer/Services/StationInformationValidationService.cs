using System.ComponentModel;
using Producer.DTOs;

namespace Producer.Services;

public class StationInformationValidationService
{
    public bool IsValid(StationInformationDto station)
    {
        return !string.IsNullOrWhiteSpace(station.StationId)
        && station.Latitude is >= -90 and <= 90
        && station.Longitude is >= -180 and <= 180
        && station.Capacity is >= 0;
    }
}