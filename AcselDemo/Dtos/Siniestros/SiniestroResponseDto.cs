namespace AcselDemo.Dtos.Siniestros;

public class SiniestroResponseDto
{
    public int IdSiniestro { get; set; }
    public string NumeroSiniestro { get; set; } = string.Empty;
    public int IdPoliza { get; set; }
    public DateTime FechaOcurrencia { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal MontoReclamado { get; set; }
    public decimal MontoDeducible { get; set; }
    public decimal MontoIndemnizar { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaPago { get; set; }
}
