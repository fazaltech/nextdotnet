namespace Backend.Application.Dashboard;

public sealed record DashboardSummaryDto(
    int PendingOrders,
    decimal TodaySales,
    int TotalCustomers,
    int TotalMenuItems,
    int LowStockItems,
    decimal TodayExpenses);

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
