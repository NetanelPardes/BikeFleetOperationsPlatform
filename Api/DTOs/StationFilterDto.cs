namespace Api.DTOs;

public class StationFilterDto
{
    public string? Status { get; set; }
    public int? MinAvailableBikes { get; set; }
    public bool? IsRenting { get; set; }
    public bool? IsReturning { get; set; }
}