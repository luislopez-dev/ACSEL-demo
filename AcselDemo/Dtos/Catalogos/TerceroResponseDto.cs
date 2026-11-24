namespace AcselDemo.Dtos.Catalogos;

public class TerceroResponseDto
{
    public int IdTercero { get; set; }
    public string DocumentoFiscal { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Correo { get; set; }
}
