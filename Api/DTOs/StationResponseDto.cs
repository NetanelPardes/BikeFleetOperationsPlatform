namespace Api.DTOs;

public class StationResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Capacity { get; set; }
    public int? AvailableBikes { get; set; }
    public int? AvailableDocks { get; set; }
    public string Status { get; set; } = "unknown";
    public bool? IsRenting { get; set; }
    public bool? IsReturning { get; set; }
    public long? LastReported { get; set; }
}