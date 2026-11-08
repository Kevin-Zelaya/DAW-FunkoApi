using CSharpFunctionalExtensions;

public interface IFunkoPriceHistoryService
{
    Task<Result<PriceHistoryModifiedResponse, DomainError>> AddHistoryAsync(long funkoId, decimal price, decimal oldPrice, CancellationToken ct);
    Task<Result<PriceHistoryModifiedResponse, DomainError>> DeleteHistoryAsync(long funkoId, CancellationToken ct);
    Task<Result<FunkoPrieceHistoryResponsesDto, DomainError>> GetHistoryAsync(long id, CancellationToken ct);
    Task<Result<PriceHistoryModifiedResponse, DomainError>> RestoreHistoryAsync(long funkoId, CancellationToken ct);
}