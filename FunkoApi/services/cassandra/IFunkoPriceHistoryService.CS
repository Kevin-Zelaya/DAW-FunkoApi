using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones de servicio para la gestión del historial de precios.
/// </summary>
public interface IFunkoPriceHistoryService
{
    /// <summary>
    /// Añade un nuevo registro al historial de precios.
    /// </summary>
    /// <param name="funkoId">ID del funko.</param>
    /// <param name="price">Precio actual.</param>
    /// <param name="oldPrice">Precio anterior.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación o un error de dominio.</returns>
    Task<Result<PriceHistoryModifiedResponse, DomainError>> AddHistoryAsync(long funkoId, decimal price, decimal oldPrice, CancellationToken ct);

    /// <summary>
    /// Elimina el historial de precios de un funko.
    /// </summary>
    /// <param name="funkoId">ID del funko.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<CSharpFunctionalExtensions.Result<PriceHistoryModifiedResponse>> DeleteHistoryAsync(long funkoId, CancellationToken ct);

    /// <summary>
    /// Obtiene el historial de precios de un funko.
    /// </summary>
    /// <param name="id">ID del funko.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Historial de precios o un error de dominio.</returns>
    Task<Result<FunkoPrieceHistoryResponsesDto, DomainError>> GetHistoryAsync(long id, CancellationToken ct);

    /// <summary>
    /// Restaura el historial de precios de un funko.
    /// </summary>
    /// <param name="funkoId">ID del funko.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<CSharpFunctionalExtensions.Result<PriceHistoryModifiedResponse>> RestoreHistoryAsync(long funkoId, CancellationToken ct);
}