using Backend.Application.Persistence;
using Backend.Application.Reports;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class ReportRepository(IDbConnectionFactory connectionFactory) : IReportRepository
{
    public async Task<DailySalesReportDto> GetDailySalesAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);

        const string orderSql = @"
SELECT COUNT(DISTINCT OrderId) AS TotalOrders,
       ISNULL(SUM((COALESCE(Subtotal,TotalAmount)-Discount)*(1-CASE WHEN TotalAmount>0 THEN RefundAmount/TotalAmount ELSE 0 END)), 0) AS GrossSales
FROM dbo.RmsInvoices
WHERE InvoiceDateUtc >= @StartDate
  AND InvoiceDateUtc < @EndDate
  AND Status<>4 AND IsActive = 1;";

        var orderRow = await connection.QuerySingleAsync<OrderAggRow>(new CommandDefinition(orderSql,
            new { StartDate = start, EndDate = end }, cancellationToken: cancellationToken));

        const string expenseSql = @"
SELECT ISNULL(SUM(Amount), 0)
FROM dbo.RmsExpenses
WHERE ExpenseDateUtc >= @StartDate
  AND ExpenseDateUtc < @EndDate
  AND IsActive = 1;";

        var totalExpenses = await connection.ExecuteScalarAsync<decimal>(new CommandDefinition(expenseSql,
            new { StartDate = start, EndDate = end }, cancellationToken: cancellationToken));

        var ingredientCost=await connection.ExecuteScalarAsync<decimal>(new CommandDefinition("SELECT COALESCE(SUM(CASE WHEN TransactionType=1 THEN -Quantity ELSE Quantity END*COALESCE(UnitCost,0)),0) FROM dbo.RmsStockTransactions WHERE MovementKind IN ('Consumption','Reversal','Waste') AND CreatedAtUtc>=@StartDate AND CreatedAtUtc<@EndDate",new { StartDate=start,EndDate=end },cancellationToken:cancellationToken));
        var grossProfit=orderRow.GrossSales-ingredientCost;
        return new DailySalesReportDto(date, orderRow.TotalOrders, orderRow.GrossSales, totalExpenses, orderRow.GrossSales,ingredientCost,grossProfit,grossProfit-totalExpenses);
    }

    private sealed class OrderAggRow
    {
        public int TotalOrders { get; init; }
        public decimal GrossSales { get; init; }
    }
}

