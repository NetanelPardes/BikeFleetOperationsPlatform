using System.Text.Json;
using System.Text.Json.Serialization;

namespace Consumer.Models;

public class StationInformation
{
    [JsonPropertyName("station_id")]
    public string? StationId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("lat")]
    public double? Latitude { get; set; }

    [JsonPropertyName("lon")]
    public double? Longitude { get; set; }

    [JsonPropertyName("capacity")]
    public int? Capacity { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}