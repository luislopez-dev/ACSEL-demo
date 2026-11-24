using AcselDemo.Dtos.Siniestros;

namespace AcselDemo.Interfaces;

public interface ISiniestroService
{
    Task<RegistrarSiniestroResponseDto> RegistrarAsync(RegistrarSiniestroRequestDto request, CancellationToken cancellationToken = default);
    Task PagarAsync(int idSiniestro, CancellationToken cancellationToken = default);
}
