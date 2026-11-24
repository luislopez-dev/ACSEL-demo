using System.ComponentModel.DataAnnotations;

namespace AcselDemo.Dtos.Polizas;

public class EmitirPolizaRequestDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un asegurado.")]
    public int IdTercero { get; set; }

    [Required(ErrorMessage = "La descripción del bien asegurado es obligatoria.")]
    [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string DescripcionBien { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime FechaDesde { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateTime FechaHasta { get; set; }

    [Range(1, 12, ErrorMessage = "Seleccione una cantidad de cuotas válida.")]
    public int CantidadCuotas { get; set; } = 1;

    [Range(1, 100_000_000, ErrorMessage = "La suma asegurada debe ser mayor a cero.")]
    public decimal SumaAsegurada { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaHasta.Date <= FechaDesde.Date)
        {
            yield return new ValidationResult(
                "La fecha fin debe ser posterior a la fecha inicio.",
                [nameof(FechaHasta)]);
        }
    }
}
