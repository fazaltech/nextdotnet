using Backend.Application.Dashboard;
using Backend.Application.Persistence;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class DashboardRepository(IDbConnectionFactory connectionFactory) : IDashboardRepository
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);

        var pendingOrders = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM dbo.RmsSalesOrders WHERE Status IN (1,2,3) AND IsActive = 1;",
            cancellationToken: cancellationToken));

        var todaySales = await connection.ExecuteScalarAsync<decimal>(new CommandDefinition(@"
SELECT ISNULL(SUM(TotalAmount), 0)
FROM dbo.RmsInvoices
WHERE InvoiceDateUtc >= @StartDate
  AND InvoiceDateUtc < @EndDate
  AND IsActive = 1;",
            new { StartDate = todayStart, EndDate = tomorrowStart }, cancellationToken: cancellationToken));

        var totalCustomers = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM dbo.RmsCustomers WHERE IsActive = 1;",
            cancellationToken: cancellationToken));

        var totalMenuItems = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM dbo.RmsMenuItems WHERE IsActive = 1;",
            cancellationToken: cancellationToken));

        var lowStockItems = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM dbo.RmsInventoryItems WHERE IsActive = 1 AND Quantity <= ReorderLevel;",
            cancellationToken: cancellationToken));

        var todayExpenses = await connection.ExecuteScalarAsync<decimal>(new CommandDefinition(@"
SELECT ISNULL(SUM(Amount), 0)
FROM dbo.RmsExpenses
WHERE ExpenseDateUtc >= @StartDate
  AND ExpenseDateUtc < @EndDate
  AND IsActive = 1;",
            new { StartDate = todayStart, EndDate = tomorrowStart }, cancellationToken: cancellationToken));

        return new DashboardSummaryDto(pendingOrders, todaySales, totalCustomers, totalMenuItems, lowStockItems, todayExpenses);
    }
}

