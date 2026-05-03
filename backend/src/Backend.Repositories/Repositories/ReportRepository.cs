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
SELECT COUNT(1) AS TotalOrders,
       ISNULL(SUM(TotalAmount), 0) AS GrossSales
FROM dbo.RmsSalesOrders
WHERE OrderDateUtc >= @StartDate
  AND OrderDateUtc < @EndDate
  AND IsActive = 1;";

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

        return new DailySalesReportDto(date, orderRow.TotalOrders, orderRow.GrossSales, totalExpenses, orderRow.GrossSales - totalExpenses);
    }

    private sealed class OrderAggRow
    {
        public int TotalOrders { get; init; }
        public decimal GrossSales { get; init; }
    }
}

