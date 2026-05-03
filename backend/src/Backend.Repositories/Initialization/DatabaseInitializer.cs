using Backend.Repositories.Database;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Repositories.Initialization;

public sealed class DatabaseInitializer(
    ApplicationDbContext dbContext,
    IDbConnectionFactory connectionFactory) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureIdentityTablesAsync(cancellationToken);

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(DatabaseSchema.Sql, cancellationToken: cancellationToken));
    }

    private async Task EnsureIdentityTablesAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        const string identityExistsSql = @"
SELECT CASE WHEN EXISTS(
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'AspNetRoles'
) THEN 1 ELSE 0 END;";

        var hasIdentityTables = await connection.ExecuteScalarAsync<int>(new CommandDefinition(identityExistsSql, cancellationToken: cancellationToken)) == 1;
        if (hasIdentityTables)
        {
            return;
        }

        var creator = dbContext.GetService<IRelationalDatabaseCreator>();

        if (!await creator.ExistsAsync(cancellationToken))
        {
            await creator.CreateAsync(cancellationToken);
        }

        await creator.CreateTablesAsync(cancellationToken);
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
