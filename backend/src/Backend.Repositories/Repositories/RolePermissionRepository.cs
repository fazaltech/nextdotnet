using Backend.Application.Persistence;
using Backend.Application.Roles;
using Backend.Repositories.Database;
using Dapper;

namespace Backend.Repositories.Repositories;

public sealed class RolePermissionRepository(IDbConnectionFactory connectionFactory) : IRolePermissionRepository
{
    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT Id, Name, Description
FROM dbo.RmsPermissions
WHERE IsActive = 1
ORDER BY Name;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<PermissionDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task<IReadOnlyList<int>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT PermissionId
FROM dbo.RmsRolePermissions
WHERE RoleId = @RoleId;";

        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<int>(new CommandDefinition(sql, new { RoleId = roleId }, cancellationToken: cancellationToken));
        return rows.ToArray();
    }

    public async Task UpdateRolePermissionsAsync(int roleId, IReadOnlyList<int> permissionIds, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionFactory.OpenSqlConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string deleteSql = "DELETE FROM dbo.RmsRolePermissions WHERE RoleId = @RoleId;";
            await connection.ExecuteAsync(new CommandDefinition(deleteSql, new { RoleId = roleId }, transaction, cancellationToken: cancellationToken));

            if (permissionIds.Count > 0)
            {
                const string insertSql = @"
INSERT INTO dbo.RmsRolePermissions(RoleId, PermissionId)
VALUES (@RoleId, @PermissionId);";

                foreach (var permissionId in permissionIds.Distinct())
                {
                    await connection.ExecuteAsync(new CommandDefinition(insertSql,
                        new { RoleId = roleId, PermissionId = permissionId },
                        transaction,
                        cancellationToken: cancellationToken));
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

