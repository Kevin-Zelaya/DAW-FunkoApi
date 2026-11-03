using MinimalApi.Models; 

namespace MinimalApi.repositories;

public interface IFunkoPriceHistoryRepository
{
    Task InsertarHistorialAsync(long funkoId, double precio, double precioAnterior);
    Task<IEnumerable<HistorialPrecioFunkoModel>> ObtenerHistorialPorFunkoAsync(long funkoId);
}