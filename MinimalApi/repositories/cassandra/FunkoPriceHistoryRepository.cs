using Cassandra;
using MinimalApi.models;
using ISession = Cassandra.ISession;

namespace MinimalApi.repositories;

public class FunkoPriceHistoryRepository(ISession _session) : IFunkoPriceHistoryRepository
{
    public async Task InsertarHistorialAsync(long funkoId, decimal precioNuevo, decimal precioAnterior)
    {
       
        var insertCql = @"
            INSERT INTO historial_precios_por_funko (funko_id, fecha, precio, precio_anterior)
            VALUES (?, ?, ?, ?);";

        var prepared = await _session.PrepareAsync(insertCql);
        var statement = prepared.Bind(funkoId, DateTimeOffset.UtcNow, precioNuevo, precioAnterior);

        await _session.ExecuteAsync(statement);
    }

    public async Task<IEnumerable<HistorialPrecioFunkoModel>> ObtenerHistorialPorFunkoAsync(long funkoId)
    {
        var selectCql = @"
            SELECT funko_id, fecha, precio, precio_anterior 
            FROM historial_precios_por_funko 
            WHERE funko_id = ?;";

        var prepared = await _session.PrepareAsync(selectCql);
        var statement = prepared.Bind(funkoId);
        var rs = await _session.ExecuteAsync(statement);

        return rs.Select(row => new HistorialPrecioFunkoModel
        {
            FunkoId = row.GetValue<long>("funko_id"),
            Fecha = row.GetValue<DateTimeOffset>("fecha"),
            Precio = row.GetValue<decimal>("precio"),
            PrecioAnterior = row.GetValue<decimal>("precio_anterior")
        }).ToList();
    }
}