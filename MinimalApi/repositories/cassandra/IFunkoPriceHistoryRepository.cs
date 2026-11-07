using MinimalApi.models;

namespace MinimalApi.repositories;

public interface IFunkoPriceHistoryRepository
{
    Task AddPriceAsync(long funkoId, decimal precioNuevo, decimal precioAnterior, CancellationToken ct);
    Task<List<PriceHistoryDto>> GetHistoryByFunkoIdAsync(long funkoId, CancellationToken ct);
    Task RestoreHistoryByFunkoId(long funkoId, CancellationToken ct);
    Task DeleteHistoryByFunkoId(long funkoId, CancellationToken ct);
}