namespace AcselDemo.Dtos.Catalogos;

public class ProductoResponseDto
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal TasaAnualPct { get; set; }
    public decimal PctImpuesto { get; set; }
    public decimal PctDeducible { get; set; }
}
