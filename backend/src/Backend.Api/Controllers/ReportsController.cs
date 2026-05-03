using Backend.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("daily-sales")]
    public Task<DailySalesReportDto> GetDailySales([FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return reportService.GetDailySalesAsync(reportDate, cancellationToken);
    }
}
