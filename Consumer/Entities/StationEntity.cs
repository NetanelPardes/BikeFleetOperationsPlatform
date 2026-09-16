namespace Consumer.Entities;

public class StationEntity
{
    public string StationId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Capacity { get; set; }
    public string? AdditionalFieldsJson { get; set; }
}