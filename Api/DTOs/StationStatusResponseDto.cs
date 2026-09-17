namespace Api.DTOs;

public class StationStatusResponseDto
{
    public string StationId { get; set; } = string.Empty;

    public int? AvailableBikes { get; set; }

    public int? AvailableDocks { get; set; }

    public bool? IsRenting { get; set; }

    public bool? IsReturning { get; set; }

    public string Status { get; set; } = "unknown";

    public long? LastReported { get; set; }
}