using Backend.Application.Expenses;
using Backend.Application.Persistence;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class ExpenseRepository(IDbConnectionFactory connectionFactory) : IExpenseRepository
{
    public async Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, Title, Category, Amount, ExpenseDateUtc, Notes, IsActive
FROM dbo.RmsExpenses
ORDER BY ExpenseDateUtc DESC, Id DESC;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ExpenseDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, Title, Category, Amount, ExpenseDateUtc, Notes, IsActive
FROM dbo.RmsExpenses
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ExpenseDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(ExpenseUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.RmsExpenses(Title, Category, Amount, ExpenseDateUtc, Notes, IsActive)
VALUES (@Title, @Category, @Amount, @ExpenseDateUtc, @Notes, @IsActive);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(int id, ExpenseUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.RmsExpenses
SET Title = @Title,
    Category = @Category,
    Amount = @Amount,
    ExpenseDateUtc = @ExpenseDateUtc,
    Notes = @Notes,
    IsActive = @IsActive,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = id, request.Title, request.Category, request.Amount, request.ExpenseDateUtc, request.Notes, request.IsActive },
            cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"DELETE FROM dbo.RmsExpenses WHERE Id = @Id;";
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rows > 0;
    }
}

