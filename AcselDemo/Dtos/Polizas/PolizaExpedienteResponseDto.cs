using AcselDemo.Dtos.Siniestros;

namespace AcselDemo.Dtos.Polizas;

public class PolizaExpedienteResponseDto
{
    public PolizaCabeceraResponseDto Cabecera { get; set; } = new();
    public List<ReciboResponseDto> Recibos { get; set; } = [];
    public List<SiniestroResponseDto> Siniestros { get; set; } = [];
}
