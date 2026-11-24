using Oracle.ManagedDataAccess.Client;

namespace AcselDemo.Interfaces;

public interface IOracleConnectionFactory
{
    Task<OracleConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
