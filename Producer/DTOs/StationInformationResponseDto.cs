using System.Text.Json;

namespace Producer.DTOs;
using System.Text.Json.Serialization;

public class StationInformationResponseDto
{
    [JsonPropertyName("data")]
    public StationInformationDataDto? Data {get;set;}
}

public class StationInformationDataDto
{
    [JsonPropertyName("stations")]
    public List<StationInformationDto>? Stations{get;set;}
}
public class StationInformationDto
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