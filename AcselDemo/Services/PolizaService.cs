using System.Data;
using AcselDemo.Dtos.Catalogos;
using AcselDemo.Dtos.Common;
using AcselDemo.Dtos.Polizas;
using AcselDemo.Dtos.Siniestros;
using AcselDemo.Exceptions;
using AcselDemo.Extensions;
using AcselDemo.Interfaces;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace AcselDemo.Services;

public class PolizaService : IPolizaService
{
    private readonly IOracleConnectionFactory _connectionFactory;

    public PolizaService(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ProductoResponseDto>> GetProductosAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new OracleCommand("pkg_polizas.pr_listar_productos", connection)
        {
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };

        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var items = new List<ProductoResponseDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductoResponseDto
            {
                IdProducto = reader.GetInt32Col("id_producto"),
                Codigo = reader.GetStringCol("codigo"),
                Nombre = reader.GetStringCol("nombre"),
                TasaAnualPct = reader.GetDecimalCol("tasa_anual_pct"),
                PctImpuesto = reader.GetDecimalCol("pct_impuesto"),
                PctDeducible = reader.GetDecimalCol("pct_deducible")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<TerceroResponseDto>> GetTercerosAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new OracleCommand("pkg_polizas.pr_listar_terceros", connection)
        {
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };

        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var items = new List<TerceroResponseDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TerceroResponseDto
            {
                IdTercero = reader.GetInt32Col("id_tercero"),
                DocumentoFiscal = reader.GetStringCol("documento_fiscal"),
                NombreCompleto = reader.GetStringCol("nombre_completo"),
                Correo = reader.GetNullableStringCol("correo")
            });
        }

        return items;
    }

    public async Task<PagedResultDto<PolizaResumenResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new OracleCommand("pkg_polizas.pr_listar_polizas", connection)
        {
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };

        command.Parameters.Add("p_pagina", OracleDbType.Int32, safePage, ParameterDirection.Input);
        command.Parameters.Add("p_tamano_pagina", OracleDbType.Int32, safePageSize, ParameterDirection.Input);

        var outTotalParam = new OracleParameter("p_total_registros", OracleDbType.Decimal, ParameterDirection.Output);
        command.Parameters.Add(outTotalParam);
        command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

        var items = new List<PolizaResumenResponseDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PolizaResumenResponseDto
            {
                IdPoliza = reader.GetInt32Col("id_poliza"),
                NumeroPoliza = reader.GetStringCol("numero_poliza"),
                CodigoProducto = reader.GetStringCol("codigo_producto"),
                NombreProducto = reader.GetStringCol("nombre_producto"),
                NombreAsegurado = reader.GetStringCol("nombre_asegurado"),
                DescripcionBien = reader.GetStringCol("descripcion_bien"),
                FechaDesde = reader.GetDateTimeCol("fecha_desde"),
                FechaHasta = reader.GetDateTimeCol("fecha_hasta"),
                Estado = reader.GetStringCol("estado"),
                SumaAsegurada = reader.GetDecimalCol("suma_asegurada"),
                PrimaTotal = reader.GetDecimalCol("prima_total")
            });
        }

        var totalCount = ((OracleDecimal)outTotalParam.Value).ToInt32();

        return new PagedResultDto<PolizaResumenResponseDto>
        {
            Items = items,
            Page = safePage,
            PageSize = safePageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PolizaExpedienteResponseDto?> GetByIdAsync(int idPoliza, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new OracleCommand("pkg_polizas.pr_obtener_poliza", connection)
        {
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };

        command.Parameters.Add("p_id_poliza", OracleDbType.Int32, idPoliza, ParameterDirection.Input);
        command.Parameters.Add("p_cur_poliza", OracleDbType.RefCursor, ParameterDirection.Output);
        command.Parameters.Add("p_cur_recibos", OracleDbType.RefCursor, ParameterDirection.Output);
        command.Parameters.Add("p_cur_siniestros", OracleDbType.RefCursor, ParameterDirection.Output);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var expediente = new PolizaExpedienteResponseDto
        {
            Cabecera = new PolizaCabeceraResponseDto
            {
                IdPoliza = reader.GetInt32Col("id_poliza"),
                NumeroPoliza = reader.GetStringCol("numero_poliza"),
                CodigoProducto = reader.GetStringCol("codigo_producto"),
                NombreProducto = reader.GetStringCol("nombre_producto"),
                TasaAnualPct = reader.GetDecimalCol("tasa_anual_pct"),
                PctImpuesto = reader.GetDecimalCol("pct_impuesto"),
                PctDeducible = reader.GetDecimalCol("pct_deducible"),
                DocumentoFiscal = reader.GetStringCol("documento_fiscal"),
                NombreAsegurado = reader.GetStringCol("nombre_asegurado"),
                DescripcionBien = reader.GetStringCol("descripcion_bien"),
                FechaDesde = reader.GetDateTimeCol("fecha_desde"),
                FechaHasta = reader.GetDateTimeCol("fecha_hasta"),
                Estado = reader.GetStringCol("estado"),
                CantidadCuotas = reader.GetInt32Col("cantidad_cuotas"),
                SumaAsegurada = reader.GetDecimalCol("suma_asegurada"),
                PrimaNeta = reader.GetDecimalCol("prima_neta"),
                MontoImpuesto = reader.GetDecimalCol("monto_impuesto"),
                PrimaTotal = reader.GetDecimalCol("prima_total")
            }
        };

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                expediente.Recibos.Add(new ReciboResponseDto
                {
                    IdRecibo = reader.GetInt32Col("id_recibo"),
                    IdPoliza = reader.GetInt32Col("id_poliza"),
                    NumeroRecibo = reader.GetStringCol("numero_recibo"),
                    NumeroCuota = reader.GetInt32Col("numero_cuota"),
                    FechaVencimiento = reader.GetDateTimeCol("fecha_vencimiento"),
                    MontoTotal = reader.GetDecimalCol("monto_total"),
                    Estado = reader.GetStringCol("estado"),
                    FechaCobro = reader.GetNullableDateTimeCol("fecha_cobro"),
                    ReferenciaPago = reader.GetNullableStringCol("referencia_pago")
                });
            }
        }

        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                expediente.Siniestros.Add(new SiniestroResponseDto
                {
                    IdSiniestro = reader.GetInt32Col("id_siniestro"),
                    NumeroSiniestro = reader.GetStringCol("numero_siniestro"),
                    IdPoliza = reader.GetInt32Col("id_poliza"),
                    FechaOcurrencia = reader.GetDateTimeCol("fecha_ocurrencia"),
                    Descripcion = reader.GetStringCol("descripcion"),
                    MontoReclamado = reader.GetDecimalCol("monto_reclamado"),
                    MontoDeducible = reader.GetDecimalCol("monto_deducible"),
                    MontoIndemnizar = reader.GetDecimalCol("monto_indemnizar"),
                    Estado = reader.GetStringCol("estado"),
                    FechaPago = reader.GetNullableDateTimeCol("fecha_pago")
                });
            }
        }

        return expediente;
    }

    public async Task<EmitirPolizaResponseDto> EmitirAsync(EmitirPolizaRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using var command = new OracleCommand("pkg_polizas.pr_emitir_poliza", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                Transaction = (OracleTransaction)transaction
            };

            command.Parameters.Add("p_id_producto", OracleDbType.Int32, request.IdProducto, ParameterDirection.Input);
            command.Parameters.Add("p_id_tercero", OracleDbType.Int32, request.IdTercero, ParameterDirection.Input);
            command.Parameters.Add("p_descripcion_bien", OracleDbType.Varchar2, request.DescripcionBien, ParameterDirection.Input);
            command.Parameters.Add("p_fecha_desde", OracleDbType.Date, request.FechaDesde.Date, ParameterDirection.Input);
            command.Parameters.Add("p_fecha_hasta", OracleDbType.Date, request.FechaHasta.Date, ParameterDirection.Input);
            command.Parameters.Add("p_cantidad_cuotas", OracleDbType.Int32, request.CantidadCuotas, ParameterDirection.Input);
            command.Parameters.Add("p_suma_asegurada", OracleDbType.Decimal, request.SumaAsegurada, ParameterDirection.Input);

            var outIdParam = new OracleParameter("p_id_poliza", OracleDbType.Decimal, ParameterDirection.Output);
            var outNumeroParam = new OracleParameter("p_numero_poliza", OracleDbType.Varchar2, 40)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(outIdParam);
            command.Parameters.Add(outNumeroParam);

            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new EmitirPolizaResponseDto
            {
                IdPoliza = ((OracleDecimal)outIdParam.Value).ToInt32(),
                NumeroPoliza = ((OracleString)outNumeroParam.Value).Value
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

    public async Task CobrarReciboAsync(CobrarReciboRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using var command = new OracleCommand("pkg_polizas.pr_cobrar_recibo", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                Transaction = (OracleTransaction)transaction
            };

            command.Parameters.Add("p_id_recibo", OracleDbType.Int32, request.IdRecibo, ParameterDirection.Input);
            command.Parameters.Add("p_referencia_pago", OracleDbType.Varchar2, request.ReferenciaPago, ParameterDirection.Input);

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
