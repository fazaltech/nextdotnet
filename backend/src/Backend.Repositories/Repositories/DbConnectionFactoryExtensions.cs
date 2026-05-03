using Backend.Repositories.Database;
using Microsoft.Data.SqlClient;

namespace Backend.Repositories.Repositories;

internal static class DbConnectionFactoryExtensions
{
    public static async Task<SqlConnection> OpenSqlConnectionAsync(this IDbConnectionFactory connectionFactory, CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
