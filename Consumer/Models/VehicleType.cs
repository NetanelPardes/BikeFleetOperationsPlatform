using System.Text.Json;
using System.Text.Json.Serialization;

namespace Consumer.Models;

public class VehicleType
{
    [JsonPropertyName("vehicle_type_id")]
    public string? VehicleTypeId { get; set; }

    [JsonPropertyName("form_factor")]
    public string? FormFactor { get; set; }

    [JsonPropertyName("propulsion_type")]
    public string? PropulsionType { get; set; }

    [JsonPropertyName("max_range_meters")]
    public double? MaxRangeMeters { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}