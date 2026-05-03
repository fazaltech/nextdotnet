using Backend.Application.Persistence;
using Backend.Application.Tables;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class TableRepository(IDbConnectionFactory connectionFactory) : ITableRepository
{
    public async Task<IReadOnlyList<DiningTableDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, TableNumber, Capacity, IsOccupied, IsActive
FROM dbo.RmsDiningTables
ORDER BY TableNumber;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<DiningTableDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<DiningTableDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, TableNumber, Capacity, IsOccupied, IsActive
FROM dbo.RmsDiningTables
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<DiningTableDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(DiningTableUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.RmsDiningTables(TableNumber, Capacity, IsOccupied, IsActive)
VALUES (@TableNumber, @Capacity, @IsOccupied, @IsActive);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(int id, DiningTableUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.RmsDiningTables
SET TableNumber = @TableNumber,
    Capacity = @Capacity,
    IsOccupied = @IsOccupied,
    IsActive = @IsActive,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = id, request.TableNumber, request.Capacity, request.IsOccupied, request.IsActive },
            cancellationToken: cancellationToken));

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"DELETE FROM dbo.RmsDiningTables WHERE Id = @Id;";
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> SetOccupiedAsync(int id, bool isOccupied, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.RmsDiningTables
SET IsOccupied = @IsOccupied,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, IsOccupied = isOccupied }, cancellationToken: cancellationToken));
        return rows > 0;
    }
}

