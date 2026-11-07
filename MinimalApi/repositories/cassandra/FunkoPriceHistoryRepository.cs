using Cassandra;
using MinimalApi.models;
using Serilog;
using ISession = Cassandra.ISession;

namespace MinimalApi.repositories;

public class FunkoPriceHistoryRepository(ISession _session) : IFunkoPriceHistoryRepository
{
    public async Task AddPriceAsync(long funkoId, decimal precioNuevo, decimal precioAnterior, CancellationToken ct)
    {
       
        var insertCql = @"
            INSERT INTO funko_price_history (funko_id, date, price, old_price, deleted)
            VALUES (?, ?, ?, ?, ?);";

        var prepared = await _session.PrepareAsync(insertCql);
        var statement = prepared.Bind(funkoId, DateTimeOffset.UtcNow, precioNuevo, precioAnterior, false);

        await _session.ExecuteAsync(statement);
    }

    public async Task<List<PriceHistoryDto>> GetHistoryByFunkoIdAsync(long funkoId, CancellationToken ct)
    {
        var selectCql = @"
            SELECT funko_id, date, price, old_price 
            FROM funko_price_history
            WHERE funko_id = ? AND deleted = false
            ALLOW FILTERING;";

        var prepared = await _session.PrepareAsync(selectCql);
        var statement = prepared.Bind(funkoId);
        var rs = await _session.ExecuteAsync(statement);

        return rs.Select(row => new PriceHistoryDto
        {
            FunkoId = row.GetValue<long>("funko_id"),
            Date = row.GetValue<DateTime>("date"),
            Price = row.GetValue<decimal>("price"),
            OldPrice = row.GetValue<decimal>("old_price")
        }).ToList();
    }
    // queda pendiente eliminar todos los registros de 
    public async Task DeleteHistoryByFunkoId(long funkoId, CancellationToken ct)
    {
        var delete = @"
            UPDATE funko_price_history 
            SET deleted = true 
            WHERE funko_id = ?;
        ";
        try
        {
            var prepared = await _session.PrepareAsync(delete);
            var statement = prepared.Bind(funkoId);
            await _session.ExecuteAsync(statement); 
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[CASSANDRA-REPOSITORY] Error al eliminar los registros de precio del Funko ID {FunkoId}", funkoId);
        }
    }
    public async Task RestoreHistoryByFunkoId(long funkoId, CancellationToken ct)
    {
        var restore = @"
            UPDATE funko_price_history 
            SET deleted = false 
            WHERE funko_id = ?;
        ";
        try
        {
            var prepared = await _session.PrepareAsync(restore);
            var statement = prepared.Bind(funkoId);
            await _session.ExecuteAsync(statement); 
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[CASSANDRA-REPOSITORY] Error al restaurar los registros de precio del Funko ID {FunkoId}", funkoId);
        }
    }
}