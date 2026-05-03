using Backend.Application.MenuItems;
using Backend.Application.Persistence;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class MenuItemRepository(IDbConnectionFactory connectionFactory) : IMenuItemRepository
{
    public async Task<IReadOnlyList<MenuItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT mi.Id,
       mi.CategoryId,
       fc.Name AS CategoryName,
       mi.Name,
       mi.Description,
       mi.Price,
       mi.IsAvailable,
       mi.IsActive
FROM dbo.RmsMenuItems mi
INNER JOIN dbo.RmsFoodCategories fc ON fc.Id = mi.CategoryId
ORDER BY mi.Name;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<MenuItemDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<MenuItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT mi.Id,
       mi.CategoryId,
       fc.Name AS CategoryName,
       mi.Name,
       mi.Description,
       mi.Price,
       mi.IsAvailable,
       mi.IsActive
FROM dbo.RmsMenuItems mi
INNER JOIN dbo.RmsFoodCategories fc ON fc.Id = mi.CategoryId
WHERE mi.Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<MenuItemDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(MenuItemUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.RmsMenuItems(CategoryId, Name, Description, Price, IsAvailable, IsActive)
VALUES (@CategoryId, @Name, @Description, @Price, @IsAvailable, @IsActive);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(int id, MenuItemUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.RmsMenuItems
SET CategoryId = @CategoryId,
    Name = @Name,
    Description = @Description,
    Price = @Price,
    IsAvailable = @IsAvailable,
    IsActive = @IsActive,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new
            {
                Id = id,
                request.CategoryId,
                request.Name,
                request.Description,
                request.Price,
                request.IsAvailable,
                request.IsActive
            },
            cancellationToken: cancellationToken));

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"DELETE FROM dbo.RmsMenuItems WHERE Id = @Id;";
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rows > 0;
    }
}

