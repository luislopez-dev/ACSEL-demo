using AcselDemo.Dtos.Catalogos;
using AcselDemo.Dtos.Common;
using AcselDemo.Dtos.Polizas;

namespace AcselDemo.Interfaces;

public interface IPolizaService
{
    Task<IReadOnlyList<ProductoResponseDto>> GetProductosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TerceroResponseDto>> GetTercerosAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<PolizaResumenResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<PolizaExpedienteResponseDto?> GetByIdAsync(int idPoliza, CancellationToken cancellationToken = default);
    Task<EmitirPolizaResponseDto> EmitirAsync(EmitirPolizaRequestDto request, CancellationToken cancellationToken = default);
    Task CobrarReciboAsync(CobrarReciboRequestDto request, CancellationToken cancellationToken = default);
}
