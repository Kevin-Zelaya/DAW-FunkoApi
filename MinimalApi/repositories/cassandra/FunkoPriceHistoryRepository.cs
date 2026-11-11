using Cassandra;
using MinimalApi.models;
using Serilog;
using ISession = Cassandra.ISession;

namespace MinimalApi.repositories;

public class FunkoPriceHistoryRepository(ISession _session) : IFunkoPriceHistoryRepository
{
    /// <inheritdoc cref="IFunkoPriceHistoryRepository.AddPriceAsync(long, decimal, decimal, CancellationToken)"/>
    public async Task AddPriceAsync(long funkoId, decimal precioNuevo, decimal precioAnterior, CancellationToken ct)
    {
       
        var insertCql = @"
            INSERT INTO funko_price_history (funko_id, date, price, old_price, deleted)
            VALUES (?, ?, ?, ?, ?);";

        var prepared = await _session.PrepareAsync(insertCql);
        var statement = prepared.Bind(funkoId, DateTimeOffset.UtcNow, precioNuevo, precioAnterior, false);

        await _session.ExecuteAsync(statement);
    }
    /// <inheritdoc cref="IFunkoPriceHistoryRepository.GetHistoryByFunkoIdAsync(long, CancellationToken)"/>
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
    /// <inheritdoc cref="IFunkoPriceHistoryRepository.DeleteHistoryByFunkoId(long, CancellationToken)"/>
    public async Task DeleteHistoryByFunkoId(
        long funkoId,
        CancellationToken ct)
    {
        var select = await _session.PrepareAsync(
            "SELECT date FROM funko_price_history WHERE funko_id = ?"
        );

        var rows = await _session.ExecuteAsync(select.Bind(funkoId));

        var update = await _session.PrepareAsync(
            @"UPDATE funko_price_history
            SET deleted = true
            WHERE funko_id = ? AND date = ?"
        );

        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();

            var date = row.GetValue<DateTime>("date");

            await _session.ExecuteAsync(
                update.Bind(funkoId, date)
            );
        }
    }
    /// <inheritdoc cref="IFunkoPriceHistoryRepository.RestoreHistoryByFunkoId(long, CancellationToken)"/>
    public async Task RestoreHistoryByFunkoId(long funkoId, CancellationToken ct)
    {
        var select = await _session.PrepareAsync(
            "SELECT date FROM funko_price_history WHERE funko_id = ?"
        );

        var rows = await _session.ExecuteAsync(select.Bind(funkoId));

        var update = await _session.PrepareAsync(
            @"UPDATE funko_price_history
            SET deleted = false
            WHERE funko_id = ? AND date = ?"
        );

        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();

            var date = row.GetValue<DateTime>("date");

            await _session.ExecuteAsync(
                update.Bind(funkoId, date)
            );
        }
    }
}