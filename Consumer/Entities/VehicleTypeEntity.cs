namespace Consumer.Entities;

public class VehicleTypeEntity
{
    public string VehicleTypeId { get; set; } = string.Empty;
    public string? FormFactor { get; set; }
    public string? PropulsionType { get; set; }
    public double? MaxRangeMeters { get; set; }
    public string? AdditionalFieldsJson { get; set; }
}