namespace Backend.Application.Reports;

public sealed record DailySalesReportDto(DateOnly Date, int TotalOrders, decimal GrossSales, decimal TotalExpenses, decimal NetSales, decimal IngredientCost = 0, decimal GrossProfit = 0, decimal NetProfit = 0);

public interface IReportService
{
    Task<DailySalesReportDto> GetDailySalesAsync(DateOnly date, CancellationToken cancellationToken = default);
}
