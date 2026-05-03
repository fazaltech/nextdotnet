using Backend.Application.Reports;

namespace Backend.Application.Persistence;

public interface IReportRepository
{
    Task<DailySalesReportDto> GetDailySalesAsync(DateOnly date, CancellationToken cancellationToken = default);
}
