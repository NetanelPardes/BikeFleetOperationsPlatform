namespace Api.DTOs;

public class DashboardResponseDto
{
    public int TotalStations { get; set; }

    public int EmptyStations { get; set; }

    public int FullStations { get; set; }

    public int LowAvailabilityStations { get; set; }

    public int OutOfServiceStations { get; set; }
    public int AvailableStations { get; set; }
    public int UnknownStations { get; set; }
    public DateTimeOffset? LastUpdated { get; set; }
    
}