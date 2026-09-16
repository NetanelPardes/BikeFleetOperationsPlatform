using System.Text.Json.Serialization;
using System.Text.Json;

namespace Producer.DTOs;

public class VehicleTypesResponseDto
{
    [JsonPropertyName("data")]
    public VehicleTypesDataDto? Data { get; set; }
}

public class VehicleTypesDataDto
{
    [JsonPropertyName("vehicle_types")]
    public List<VehicleTypeDto>? VehicleTypes { get; set; }
}

public class VehicleTypeDto
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