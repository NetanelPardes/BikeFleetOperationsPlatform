using System.Text.Json;

namespace Api.Models;

public class VehicleType
{
    public string VehicleTypeId { get; set; } = string.Empty;
    public string? FormFactor { get; set; }
    public string? PropulsionType { get; set; }
    public string? AdditionalFieldsJson { get; set; }
}