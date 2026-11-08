using CSharpFunctionalExtensions;
using MinimalApi.repositories;
using Serilog;

public class FunkoPriceHistoryService(
    IFunkoPriceHistoryRepository _repository,
    INotificationService _notificationService
) : IFunkoPriceHistoryService
{

    public async Task<Result<PriceHistoryModifiedResponse, DomainError>> AddHistoryAsync(
        long funkoId,
        decimal price,
        decimal oldPrice,
        CancellationToken ct)
    {
        await _repository.AddPriceAsync(funkoId, price, oldPrice, ct);
        return Result.Success<PriceHistoryModifiedResponse, DomainError>(
            new PriceHistoryModifiedResponse($"Precio agregado correctamente para el funko con id {funkoId}")
        );
    }
    public async Task<Result<FunkoPrieceHistoryResponsesDto, DomainError>> GetHistoryAsync(
        long id,
        CancellationToken ct)
    {
        var response = await _repository.GetHistoryByFunkoIdAsync(id, ct);
        if (response.Count < 1)
            return Result.Failure<FunkoPrieceHistoryResponsesDto, DomainError>(
                FunkoPriceHistoryError.NotFound(id)
            );
        var toReturn = new FunkoPrieceHistoryResponsesDto(
            response,
            response.OrderByDescending(p => p.Price).First().Price,
            response.OrderBy(p => p.Price).First().Price
        );
        return Result.Success<FunkoPrieceHistoryResponsesDto, DomainError>(
            toReturn
        );
    }
    public async Task<Result<PriceHistoryModifiedResponse>> DeleteHistoryAsync(long funkoId, CancellationToken ct)
    {
        try { await _repository.DeleteHistoryByFunkoId(funkoId, ct); }
        catch (Exception ex)
        {
            Log.Error(ex, "[CASSANDRA-DELETE] Error al eliminar el historial de precios del funko con el id {id}", funkoId);
            return Result.Success(
                new PriceHistoryModifiedResponse("Error al eliminar el historial de precio vinculado al usuario")
            ).Tap(f => {_ = 
            _notificationService.NotifyResourceChangedAsync(
                new ResourceNotification<long>(
                    funkoId,
                    "Historial de precios",
                    MethodsHttp.DELETE.ToString(),
                    NotificationStatus.Error.ToString(),
                    $"Error al eliminar el historial de precios vinculado al funko con id {funkoId}"
                )
            );
        });
        }
        return Result.Success(

            new PriceHistoryModifiedResponse("Eliminado el historial de precios vinculado al funko.")
        ); 

    }
    public async Task<Result<PriceHistoryModifiedResponse>> RestoreHistoryAsync(long funkoId, CancellationToken ct)
    {
        try { await _repository.RestoreHistoryByFunkoId(funkoId, ct); }
        catch (Exception ex)
        {
            Log.Error(ex, "[CASSANDRA-RESTORE] Error al restaurar el historial de precios del funko con el id {id}", funkoId);
            return Result.Success(
                new PriceHistoryModifiedResponse("Error al eliminar el historial de precio vinculado al usuario")
            ).Tap(f => {_ = 
                _notificationService.NotifyResourceChangedAsync(
                    new ResourceNotification<long>(
                        funkoId,
                        "Historial de precios",
                        MethodsHttp.PATCH.ToString(),
                        NotificationStatus.Error.ToString(),
                        $"Error al restaurar el historial de precios vinculado al funko con id {funkoId}"
                    )
                );
            });
        }
        return new PriceHistoryModifiedResponse("Restaurado el historial de precios vinculado al funko.");
    }
}