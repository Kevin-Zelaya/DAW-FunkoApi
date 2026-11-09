using MinimalApi.models;

namespace MinimalApi.repositories;

public interface IFunkoPriceHistoryRepository
{
    Task AddPriceAsync(long funkoId, decimal precioNuevo, decimal precioAnterior, CancellationToken ct = default);
    Task<List<PriceHistoryDto>> GetHistoryByFunkoIdAsync(long funkoId, CancellationToken ct = default);
    Task RestoreHistoryByFunkoId(long funkoId, CancellationToken ct = default);
    Task DeleteHistoryByFunkoId(long funkoId, CancellationToken ct = default);
}