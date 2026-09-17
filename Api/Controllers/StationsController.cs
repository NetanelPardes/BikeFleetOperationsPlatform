using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/stations")]
public class StationsController : ControllerBase
{
    private readonly IStationService _stationService;

    public StationsController(IStationService stationService)
    {
        _stationService = stationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StationResponseDto>>> GetStations([FromQuery] StationFilterDto filters,CancellationToken cancellationToken)
    {
        if (filters.MinAvailableBikes < 0)
        {
            return BadRequest("minAvailableBikes cannot be negative.");
        }

        List<StationResponseDto> stations = await _stationService.GetStationsAsync(filters,cancellationToken);
        return Ok(stations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StationResponseDto>> GetStationsById(string id, CancellationToken cancellationToken)
    {
        StationResponseDto? station = await _stationService.GetStationByIdAsync(id, cancellationToken);
        if(station == null)
        {
            return NotFound();
        }
        return(station);
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<StationStatusResponseDto>> GetStatusByIdAsync(string id, CancellationToken cancellationToken)
    {
        StationStatusResponseDto? status = await _stationService.GetStatusByIdAsync(id,  cancellationToken);
        if(status == null)
        {
            return NotFound();
        }
        return(status);
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<StationHistoryResponseDto>>>GetStationHistory(string id,DateTime? from,DateTime? to,int limit = 100,CancellationToken cancellationToken = default)
    {
        List<StationHistoryResponseDto>? history = await _stationService.GetStationHistoryAsync(id,from,to,limit,cancellationToken);
        if (history is null)
        {
            return NotFound();
        }
        return Ok(history);
    }
}