using CSharpFunctionalExtensions;

public interface IFunkoPriceHistoryService
{
    Task<Result<PriceHistoryModifiedResponse, DomainError>> AddHistoryAsync(long funkoId, decimal price, decimal oldPrice, CancellationToken ct);
    Task<CSharpFunctionalExtensions.Result<PriceHistoryModifiedResponse>> DeleteHistoryAsync(long funkoId, CancellationToken ct);
    Task<Result<FunkoPrieceHistoryResponsesDto, DomainError>> GetHistoryAsync(long id, CancellationToken ct);
    Task<CSharpFunctionalExtensions.Result<PriceHistoryModifiedResponse>> RestoreHistoryAsync(long funkoId, CancellationToken ct);
}