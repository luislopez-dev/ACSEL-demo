using System.ComponentModel.DataAnnotations;

namespace AcselDemo.Dtos.Siniestros;

public class RegistrarSiniestroRequestDto
{
    public int IdPoliza { get; set; }

    [Required(ErrorMessage = "La fecha de ocurrencia es obligatoria.")]
    public DateTime FechaOcurrencia { get; set; }

    [Required(ErrorMessage = "La descripción del evento es obligatoria.")]
    [StringLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 100_000_000, ErrorMessage = "El monto reclamado debe ser mayor a cero.")]
    public decimal MontoReclamado { get; set; }
}
