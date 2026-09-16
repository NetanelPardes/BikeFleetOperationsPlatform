using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Consumer.Models;

public class StationStatusHistory
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("stationId")]
    public string StationId { get; set; } = string.Empty;

    [BsonElement("availableVehicles")]
    public int? AvailableVehicles { get; set; }

    [BsonElement("availableDocks")]
    public int? AvailableDocks { get; set; }

    [BsonElement("isRenting")]
    public int? IsRenting { get; set; }

    [BsonElement("isReturning")]
    public int? IsReturning { get; set; }

    [BsonElement("lastReported")]
    public long? LastReported { get; set; }

    [BsonElement("recordedAt")]
    public DateTime RecordedAt { get; set; }
}