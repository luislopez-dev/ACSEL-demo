namespace AcselDemo.Dtos.Polizas;

public class PolizaCabeceraResponseDto
{
    public int IdPoliza { get; set; }
    public string NumeroPoliza { get; set; } = string.Empty;
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public decimal TasaAnualPct { get; set; }
    public decimal PctImpuesto { get; set; }
    public decimal PctDeducible { get; set; }
    public string DocumentoFiscal { get; set; } = string.Empty;
    public string NombreAsegurado { get; set; } = string.Empty;
    public string DescripcionBien { get; set; } = string.Empty;
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int CantidadCuotas { get; set; }
    public decimal SumaAsegurada { get; set; }
    public decimal PrimaNeta { get; set; }
    public decimal MontoImpuesto { get; set; }
    public decimal PrimaTotal { get; set; }
}
