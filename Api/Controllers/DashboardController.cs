using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("/api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    [HttpGet]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard(CancellationToken cancellationToken)
    {
        DashboardResponseDto dashboardResponse = await _dashboardService.GetDashboard(cancellationToken);
        return Ok(dashboardResponse);
    }
}