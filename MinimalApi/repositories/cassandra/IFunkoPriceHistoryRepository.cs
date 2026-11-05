using MinimalApi.models;

namespace MinimalApi.repositories;

public interface IFunkoPriceHistoryRepository
{
    Task InsertarHistorialAsync(long funkoId, decimal precioNuevo, decimal precioAnterior);
    Task<IEnumerable<HistorialPrecioFunkoModel>> ObtenerHistorialPorFunkoAsync(long funkoId);
    Task RestoreHistoryByFunkoId(long funkoId);
    Task DeleteHistoryByFunkoId(long funkoId);
}