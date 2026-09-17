using System.Text.Json;
using System.Text.Json.Serialization;

namespace Consumer.Models;

public class StationStatus
{
    [JsonPropertyName("station_id")]
    public string? StationId { get; set; }

    [JsonPropertyName("num_vehicles_available")]
    public int? AvailableVehicles { get; set; }

    [JsonPropertyName("num_docks_available")]
    public int? AvailableDocks { get; set; }

    [JsonPropertyName("is_renting")]
    public int? IsRenting { get; set; }

    [JsonPropertyName("is_returning")]
    public int? IsReturning { get; set; }

    [JsonPropertyName("last_reported")]
    public long? LastReported { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}