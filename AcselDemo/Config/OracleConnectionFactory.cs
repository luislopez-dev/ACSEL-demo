using AcselDemo.Interfaces;
using Oracle.ManagedDataAccess.Client;

namespace AcselDemo.Services;

public class OracleConnectionFactory : IOracleConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("La cadena de conexión a Oracle no está configurada.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public async Task<OracleConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
