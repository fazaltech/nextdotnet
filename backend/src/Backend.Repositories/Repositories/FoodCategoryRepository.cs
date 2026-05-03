using Backend.Application.FoodCategories;
using Backend.Application.Persistence;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class FoodCategoryRepository(IDbConnectionFactory connectionFactory) : IFoodCategoryRepository
{
    public async Task<IReadOnlyList<FoodCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, Name, Description, IsActive
FROM dbo.RmsFoodCategories
ORDER BY Name;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<FoodCategoryDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<FoodCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, Name, Description, IsActive
FROM dbo.RmsFoodCategories
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<FoodCategoryDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(FoodCategoryUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.RmsFoodCategories(Name, Description, IsActive)
VALUES (@Name, @Description, @IsActive);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(int id, FoodCategoryUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.RmsFoodCategories
SET Name = @Name,
    Description = @Description,
    IsActive = @IsActive,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, request.Name, request.Description, request.IsActive }, cancellationToken: cancellationToken));
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"DELETE FROM dbo.RmsFoodCategories WHERE Id = @Id;";
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rows > 0;
    }
}

