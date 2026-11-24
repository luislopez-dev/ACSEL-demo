using System.ComponentModel.DataAnnotations;

namespace AcselDemo.Dtos.Polizas;

public class CobrarReciboRequestDto
{
    [Range(1, int.MaxValue)]
    public int IdRecibo { get; set; }

    [Required(ErrorMessage = "La referencia de pago es obligatoria.")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres.")]
    public string ReferenciaPago { get; set; } = string.Empty;
}
