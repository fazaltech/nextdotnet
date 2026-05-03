using Backend.Application.Persistence;
using Backend.Application.Settings;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class SettingsRepository(IDbConnectionFactory connectionFactory) : ISettingsRepository
{
    public async Task<IReadOnlyList<AppSettingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, SettingKey, SettingValue, Description, IsActive
FROM dbo.RmsAppSettings
ORDER BY SettingKey;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<AppSettingDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<AppSettingDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, SettingKey, SettingValue, Description, IsActive
FROM dbo.RmsAppSettings
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AppSettingDto>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(AppSettingUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO dbo.RmsAppSettings(SettingKey, SettingValue, Description, IsActive)
VALUES (@SettingKey, @SettingValue, @Description, @IsActive);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, request, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(int id, AppSettingUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
UPDATE dbo.RmsAppSettings
SET SettingKey = @SettingKey,
    SettingValue = @SettingValue,
    Description = @Description,
    IsActive = @IsActive,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE Id = @Id;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = id, request.SettingKey, request.SettingValue, request.Description, request.IsActive },
            cancellationToken: cancellationToken));

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"DELETE FROM dbo.RmsAppSettings WHERE Id = @Id;";
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rows > 0;
    }
}

