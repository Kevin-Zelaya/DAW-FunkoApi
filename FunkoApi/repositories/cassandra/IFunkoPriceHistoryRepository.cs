using MinimalApi.models;

namespace MinimalApi.repositories;

/// <summary>
/// Define las operaciones para gestionar el historial de precios de los Funkos.
/// </summary>
public interface IFunkoPriceHistoryRepository
{
    /// <summary>
    /// Añade un nuevo registro al historial de precios.
    /// </summary>
    /// <param name="funkoId">ID del Funko.</param>
    /// <param name="precioNuevo">Nuevo precio del Funko.</param>
    /// <param name="precioAnterior">Precio anterior del Funko.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddPriceAsync(long funkoId, decimal precioNuevo, decimal precioAnterior, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el historial de precios de un Funko.
    /// </summary>
    /// <param name="funkoId">ID del Funko.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista con el historial de precios del Funko.</returns>
    Task<List<PriceHistoryDto>> GetHistoryByFunkoIdAsync(long funkoId, CancellationToken ct = default);

    /// <summary>
    /// Restaura los registros eliminados del historial de precios de un Funko.
    /// </summary>
    /// <param name="funkoId">ID del Funko.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task RestoreHistoryByFunkoId(long funkoId, CancellationToken ct = default);

    /// <summary>
    /// Elimina lógicamente todos los registros del historial de precios de un Funko.
    /// </summary>
    /// <param name="funkoId">ID del Funko.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task DeleteHistoryByFunkoId(long funkoId, CancellationToken ct = default);
}