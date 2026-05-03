using System.Data;

namespace Backend.Repositories.Database;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
