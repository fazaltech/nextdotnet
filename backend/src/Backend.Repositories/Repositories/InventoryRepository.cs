using Backend.Application.Inventory;
using Backend.Application.Persistence;
using Backend.Domain.Enums;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class InventoryRepository(IDbConnectionFactory connectionFactory) : IInventoryRepository
{
    public async Task<IReadOnlyList<InventoryItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, Name, Unit, Quantity, ReorderLevel, UnitCost, IsActive
FROM dbo.RmsInventoryItems
ORDER BY Name;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<InventoryItemDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<InventoryItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, Name, Unit, Quantity, ReorderLevel, UnitCost, IsActive
FROM dbo.RmsInventoryItems
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<InventoryItemDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(InventoryItemUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if(request.Quantity!=0) throw new InvalidOperationException("Create inventory at zero, then record an opening count with a reason.");
        if(request.UnitCost<0 || request.ReorderLevel<0 || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Unit)) throw new InvalidOperationException("Name, unit and nonnegative costs/thresholds required.");
        const string sql = @"
INSERT INTO dbo.RmsInventoryItems(Name, Unit, Quantity, ReorderLevel, UnitCost, IsActive)
VALUES (@Name, @Unit, 0, @ReorderLevel, @UnitCost, @IsActive);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(int id, InventoryItemUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.RmsInventoryItems
SET Name = @Name,
    Unit = @Unit,
    ReorderLevel = @ReorderLevel,
    UnitCost = @UnitCost,
    IsActive = @IsActive,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id AND Quantity=@Quantity AND (Unit=@Unit OR NOT EXISTS(SELECT 1 FROM dbo.RmsStockTransactions WHERE InventoryItemId=@Id)) AND (Unit=@Unit OR NOT EXISTS(SELECT 1 FROM dbo.RmsMenuItemRecipes WHERE InventoryItemId=@Id));";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = id, request.Name, request.Unit, request.Quantity, request.ReorderLevel, request.UnitCost, request.IsActive },
            cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"UPDATE dbo.RmsInventoryItems SET IsActive=0 WHERE Id=@Id AND Quantity=0;";
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<int> CreateStockTransactionAsync(StockTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string insertSql = @"
INSERT INTO dbo.RmsStockTransactions(InventoryItemId, Quantity, TransactionType, Notes, IsActive)
VALUES (@InventoryItemId, @Quantity, @TransactionType, @Notes, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var transactionId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(insertSql,
                new
                {
                    request.InventoryItemId,
                    request.Quantity,
                    TransactionType = (int)request.TransactionType,
                    request.Notes
                },
                transaction,
                cancellationToken: cancellationToken));

            var sign = request.TransactionType == StockTransactionType.Out ? -1 : 1;

            const string updateSql = @"
UPDATE dbo.RmsInventoryItems
SET Quantity = Quantity + (@Sign * @Quantity),
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @InventoryItemId;";

            await connection.ExecuteAsync(new CommandDefinition(updateSql,
                new
                {
                    request.InventoryItemId,
                    request.Quantity,
                    Sign = sign
                },
                transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
            return transactionId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

