using Api.DTOs;

namespace Api.Services;

public interface IDashboardService
{
    Task<DashboardResponseDto> GetDashboard(CancellationToken cancellationToken = default);
}