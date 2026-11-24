namespace AcselDemo.Dtos.Polizas;

public class ReciboResponseDto
{
    public int IdRecibo { get; set; }
    public int IdPoliza { get; set; }
    public string NumeroRecibo { get; set; } = string.Empty;
    public int NumeroCuota { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal MontoTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaCobro { get; set; }
    public string? ReferenciaPago { get; set; }
}
