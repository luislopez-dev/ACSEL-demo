using AcselDemo.Interfaces;
using AcselDemo.Services;

namespace AcselDemo.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddOracleInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OracleCore")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'ConnectionStrings:OracleCore'.");

        services.AddSingleton<IOracleConnectionFactory>(_ => new OracleConnectionFactory(connectionString));
        services.AddScoped<IPolizaService, PolizaService>();
        services.AddScoped<ISiniestroService, SiniestroService>();

        return services;
    }
}
