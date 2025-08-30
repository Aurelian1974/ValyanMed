using System.Data;

namespace Infrastructure.Data;

public interface ISqlConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
    IDbConnection CreateConnection();
}