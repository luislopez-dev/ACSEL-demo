namespace AcselDemo.Dtos.Polizas;

public class PolizaResumenResponseDto
{
    public int IdPoliza { get; set; }
    public string NumeroPoliza { get; set; } = string.Empty;
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreAsegurado { get; set; } = string.Empty;
    public string DescripcionBien { get; set; } = string.Empty;
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal SumaAsegurada { get; set; }
    public decimal PrimaTotal { get; set; }
}
