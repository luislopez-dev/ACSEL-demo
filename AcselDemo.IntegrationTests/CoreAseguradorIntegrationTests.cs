using AcselDemo.Dtos.Polizas;
using AcselDemo.Dtos.Siniestros;
using AcselDemo.Exceptions;
using AcselDemo.IntegrationTests.Infrastructure;

namespace AcselDemo.IntegrationTests;

[Collection(OracleDatabaseCollection.Name)]
public class CoreAseguradorIntegrationTests
{
    private readonly OracleContainerFixture _fixture;

    public CoreAseguradorIntegrationTests(OracleContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EmitirPoliza_En12Cuotas_CalculaTarifaYCuadraCentavosEnRecibos()
    {
        // Producto 1 (AUTO-IND): Tasa 3.50% anual | Impuesto 13.00%
        // Suma = 17,500.00 -> Prima Neta = 612.50 | Impuesto = 79.63 | Prima Total = 692.13
        var emision = await _fixture.PolizaService.EmitirAsync(new EmitirPolizaRequestDto
        {
            IdProducto = 1,
            IdTercero = 1,
            DescripcionBien = "Toyota Hilux SRV 4x4",
            FechaDesde = new DateTime(2026, 1, 1),
            FechaHasta = new DateTime(2027, 1, 1),
            CantidadCuotas = 12,
            SumaAsegurada = 17_500.00m
        });

        Assert.True(emision.IdPoliza > 0);
        Assert.StartsWith("POL-2026-", emision.NumeroPoliza);

        var expediente = await _fixture.PolizaService.GetByIdAsync(emision.IdPoliza);
        Assert.NotNull(expediente);

        Assert.Equal("VIGENTE", expediente.Cabecera.Estado);
        Assert.Equal(612.50m, expediente.Cabecera.PrimaNeta);
        Assert.Equal(79.63m, expediente.Cabecera.MontoImpuesto);
        Assert.Equal(692.13m, expediente.Cabecera.PrimaTotal);

        // Cuadre matemático exacto en 12 cuotas (ajuste de centavos en cuota 12)
        Assert.Equal(12, expediente.Recibos.Count);
        Assert.Equal(692.13m, expediente.Recibos.Sum(r => r.MontoTotal));
        Assert.Equal(57.67m, expediente.Recibos[0].MontoTotal);
        Assert.Equal(57.76m, expediente.Recibos[11].MontoTotal);
    }

    [Fact]
    public async Task FlujoSiniestro_ValidaMoraVigenciaDeducibleYPagoEnOracle()
    {
        var emision = await _fixture.PolizaService.EmitirAsync(new EmitirPolizaRequestDto
        {
            IdProducto = 1, // Deducible 10%
            IdTercero = 2,
            DescripcionBien = "Honda CR-V EXL",
            FechaDesde = new DateTime(2026, 3, 1),
            FechaHasta = new DateTime(2027, 3, 1),
            CantidadCuotas = 4,
            SumaAsegurada = 25_000.00m
        });

        var expediente = await _fixture.PolizaService.GetByIdAsync(emision.IdPoliza);
        var primerRecibo = expediente!.Recibos.First();

        // 1. Debe rechazar por mora inicial (ningún recibo cobrado -> ORA-20012)
        var exMora = await Assert.ThrowsAsync<OracleBusinessRuleException>(() =>
            _fixture.SiniestroService.RegistrarAsync(new RegistrarSiniestroRequestDto
            {
                IdPoliza = emision.IdPoliza,
                FechaOcurrencia = new DateTime(2026, 4, 15),
                Descripcion = "Colisión en vía urbana",
                MontoReclamado = 4_000.00m
            }));
        Assert.Equal(20012, exMora.OracleErrorCode);

        // 2. Cobramos el primer recibo
        await _fixture.PolizaService.CobrarReciboAsync(new CobrarReciboRequestDto
        {
            IdRecibo = primerRecibo.IdRecibo,
            ReferenciaPago = "PAG-CAJA-01"
        });

        // 3. Debe rechazar fecha fuera de vigencia (ORA-20011)
        var exVigencia = await Assert.ThrowsAsync<OracleBusinessRuleException>(() =>
            _fixture.SiniestroService.RegistrarAsync(new RegistrarSiniestroRequestDto
            {
                IdPoliza = emision.IdPoliza,
                FechaOcurrencia = new DateTime(2026, 2, 10),
                Descripcion = "Evento previo a vigencia",
                MontoReclamado = 1_000.00m
            }));
        Assert.Equal(20011, exVigencia.OracleErrorCode);

        // 4. Registro válido: 4,000.00 reclamado - 10% deducible (400.00) = 3,600.00 a indemnizar
        var siniestro = await _fixture.SiniestroService.RegistrarAsync(new RegistrarSiniestroRequestDto
        {
            IdPoliza = emision.IdPoliza,
            FechaOcurrencia = new DateTime(2026, 4, 15),
            Descripcion = "Colisión en vía urbana",
            MontoReclamado = 4_000.00m
        });
        Assert.StartsWith("SIN-2026-", siniestro.NumeroSiniestro);

        await _fixture.SiniestroService.PagarAsync(siniestro.IdSiniestro);

        var expedienteFinal = await _fixture.PolizaService.GetByIdAsync(emision.IdPoliza);
        var sinGuardado = expedienteFinal!.Siniestros.Single();

        Assert.Equal(400.00m, sinGuardado.MontoDeducible);
        Assert.Equal(3_600.00m, sinGuardado.MontoIndemnizar);
        Assert.Equal("PAGADO", sinGuardado.Estado);
    }

    [Fact]
    public async Task ListarPolizasPaginado_RespetaTamanoPaginaYTotalEnOracle()
    {
        for (var i = 1; i <= 3; i++)
        {
            await _fixture.PolizaService.EmitirAsync(new EmitirPolizaRequestDto
            {
                IdProducto = 2,
                IdTercero = 3,
                DescripcionBien = $"Vivienda Paginada #{i}",
                FechaDesde = new DateTime(2026, 1, 1),
                FechaHasta = new DateTime(2027, 1, 1),
                CantidadCuotas = 1,
                SumaAsegurada = 30_000.00m
            });
        }

        var pagina1 = await _fixture.PolizaService.GetPagedAsync(page: 1, pageSize: 2);
        var pagina2 = await _fixture.PolizaService.GetPagedAsync(page: 2, pageSize: 2);

        Assert.True(pagina1.TotalCount >= 3);
        Assert.Equal(2, pagina1.Items.Count);
        Assert.True(pagina1.HasNextPage);
        Assert.NotEmpty(pagina2.Items);
        Assert.True(pagina2.HasPreviousPage);
    }
}
