using Backend.Application.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    public Task<DashboardSummaryDto> GetSummary(CancellationToken cancellationToken)
        => dashboardService.GetSummaryAsync(cancellationToken);
}
