using System.Text.Json.Serialization;
using System.Text.Json;

namespace Producer.DTOs;

public class StationStatusResponseDto
{
    [JsonPropertyName("data")]
    public StationStatusDataDto? Data { get; set; }
}

public class StationStatusDataDto
{
    [JsonPropertyName("stations")]
    public List<StationStatusDto>? Stations { get; set; }
}

public class StationStatusDto
{
    [JsonPropertyName("station_id")]
    public string? StationId { get; set; }

    [JsonPropertyName("num_bikes_available")]
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