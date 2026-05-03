using Backend.Application.Persistence;
using Backend.Application.Reports;

namespace Backend.Services.Reports;

public sealed class ReportService(IReportRepository repository) : IReportService
{
    public Task<DailySalesReportDto> GetDailySalesAsync(DateOnly date, CancellationToken cancellationToken = default)
        => repository.GetDailySalesAsync(date, cancellationToken);
}
