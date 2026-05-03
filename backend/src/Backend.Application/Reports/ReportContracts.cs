namespace Backend.Application.Reports;

public sealed record DailySalesReportDto(DateOnly Date, int TotalOrders, decimal GrossSales, decimal TotalExpenses, decimal NetSales);

public interface IReportService
{
    Task<DailySalesReportDto> GetDailySalesAsync(DateOnly date, CancellationToken cancellationToken = default);
}
