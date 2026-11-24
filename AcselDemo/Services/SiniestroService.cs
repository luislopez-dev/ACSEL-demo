using System.Data;
using AcselDemo.Dtos.Siniestros;
using AcselDemo.Exceptions;
using AcselDemo.Interfaces;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace AcselDemo.Services;

public class SiniestroService : ISiniestroService
{
    private readonly IOracleConnectionFactory _connectionFactory;

    public SiniestroService(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RegistrarSiniestroResponseDto> RegistrarAsync(RegistrarSiniestroRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using var command = new OracleCommand("pkg_siniestros.pr_registrar_siniestro", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                Transaction = (OracleTransaction)transaction
            };

            command.Parameters.Add("p_id_poliza", OracleDbType.Int32, request.IdPoliza, ParameterDirection.Input);
            command.Parameters.Add("p_fecha_ocurrencia", OracleDbType.Date, request.FechaOcurrencia.Date, ParameterDirection.Input);
            command.Parameters.Add("p_descripcion", OracleDbType.Varchar2, request.Descripcion, ParameterDirection.Input);
            command.Parameters.Add("p_monto_reclamado", OracleDbType.Decimal, request.MontoReclamado, ParameterDirection.Input);

            var outIdParam = new OracleParameter("p_id_siniestro", OracleDbType.Decimal, ParameterDirection.Output);
            var outNumeroParam = new OracleParameter("p_numero_siniestro", OracleDbType.Varchar2, 40)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(outIdParam);
            command.Parameters.Add(outNumeroParam);

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new RegistrarSiniestroResponseDto
            {
                IdSiniestro = ((OracleDecimal)outIdParam.Value).ToInt32(),
                NumeroSiniestro = ((OracleString)outNumeroParam.Value).Value
            };
        }
        catch (OracleException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (OracleBusinessRuleException.TryTranslate(ex, out var businessEx))
            {
                throw businessEx!;
            }

            throw;
        }
    }

    public async Task PagarAsync(int idSiniestro, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using var command = new OracleCommand("pkg_siniestros.pr_pagar_siniestro", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                Transaction = (OracleTransaction)transaction
            };

            command.Parameters.Add("p_id_siniestro", OracleDbType.Int32, idSiniestro, ParameterDirection.Input);

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (OracleException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (OracleBusinessRuleException.TryTranslate(ex, out var businessEx))
            {
                throw businessEx!;
            }

            throw;
        }
    }
}
