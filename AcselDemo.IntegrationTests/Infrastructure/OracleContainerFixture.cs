using System.Text;
using AcselDemo.Extensions;
using AcselDemo.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using Testcontainers.Oracle;

namespace AcselDemo.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public class OracleDatabaseCollection : ICollectionFixture<OracleContainerFixture>
{
    public const string Name = "OracleCoreCollection";
}

public sealed class OracleContainerFixture : IAsyncLifetime
{
    private readonly OracleContainer _oracleContainer = new OracleBuilder("gvenzl/oracle-free:23-slim")
        .Build();

    private ServiceProvider? _serviceProvider;

    public IServiceProvider Services => _serviceProvider
        ?? throw new InvalidOperationException("El contenedor de pruebas Oracle aún no ha sido inicializado.");

    public IPolizaService PolizaService => Services.GetRequiredService<IPolizaService>();
    public ISiniestroService SiniestroService => Services.GetRequiredService<ISiniestroService>();

    public async Task InitializeAsync()
    {
        await _oracleContainer.StartAsync();

        var connectionString = _oracleContainer.GetConnectionString();
        await InitializeDatabaseSchemaAsync(connectionString);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:OracleCore"] = connectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOracleInfrastructure(configuration);
        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        await _oracleContainer.DisposeAsync();
    }

    private static async Task InitializeDatabaseSchemaAsync(string connectionString)
    {
        var databaseFolder = FindDatabaseFolder();
        var scriptFiles = Directory.GetFiles(databaseFolder)
            .Where(f =>
            {
                var ext = Path.GetExtension(f);
                return ext.Equals(".sql", StringComparison.OrdinalIgnoreCase)
                    || ext.Equals(".pks", StringComparison.OrdinalIgnoreCase)
                    || ext.Equals(".pkb", StringComparison.OrdinalIgnoreCase);
            })
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        foreach (var file in scriptFiles)
        {
            var ext = Path.GetExtension(file);
            var content = await File.ReadAllTextAsync(file, Encoding.UTF8);

            if (ext.Equals(".pks", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".pkb", StringComparison.OrdinalIgnoreCase))
            {
                var plsqlBlock = StripTrailingSlash(content);
                await ExecuteCommandAsync(connection, plsqlBlock, Path.GetFileName(file));
            }
            else
            {
                foreach (var sqlStatement in SplitSqlStatements(content))
                {
                    if (sqlStatement.Equals("COMMIT", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    await ExecuteCommandAsync(connection, sqlStatement, Path.GetFileName(file));
                }
            }
        }

        await VerifyAllObjectsValidAsync(connection);
    }

    private static async Task ExecuteCommandAsync(OracleConnection connection, string commandText, string fileName)
    {
        try
        {
            await using var cmd = new OracleCommand(commandText, connection)
            {
                BindByName = false
            };
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error ejecutando '{fileName}': {ex.Message}\nSQL:\n{commandText}", ex);
        }
    }

    private static async Task VerifyAllObjectsValidAsync(OracleConnection connection)
    {
        const string errorsQuery = """
            SELECT name, type, line, position, text
              FROM user_errors
             ORDER BY name, sequence
            """;

        await using var cmd = new OracleCommand(errorsQuery, connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        var errors = new List<string>();
        while (await reader.ReadAsync())
        {
            errors.Add($"{reader.GetString(1)} {reader.GetString(0)} (L{reader.GetInt32(2)}:C{reader.GetInt32(3)}): {reader.GetString(4)}");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Errores de compilación en paquetes PL/SQL:\n" + string.Join(Environment.NewLine, errors));
        }
    }

    private static string StripTrailingSlash(string content)
    {
        var lines = content
            .Replace("\r\n", "\n")
            .Split('\n')
            .ToList();

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        if (lines.Count > 0 && lines[^1].Trim() == "/")
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return string.Join("\n", lines).Trim();
    }

    private static IEnumerable<string> SplitSqlStatements(string content)
    {
        var sb = new StringBuilder();
        var lines = content.Replace("\r\n", "\n").Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            sb.AppendLine(line);

            if (trimmed.EndsWith(';'))
            {
                var statement = sb.ToString().Trim();
                statement = statement[..^1].Trim();
                sb.Clear();

                if (!string.IsNullOrWhiteSpace(statement))
                {
                    yield return statement;
                }
            }
        }
    }

    private static string FindDatabaseFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "database");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("No se encontró la carpeta 'database' con los scripts de Oracle.");
    }
}
