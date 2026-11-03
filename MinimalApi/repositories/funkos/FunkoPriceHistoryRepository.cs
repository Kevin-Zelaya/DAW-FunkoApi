using Cassandra;
using MinimalApi.models;

namespace MinimalApi.repositories;

public class FunkoPriceHistoryRepository(Cassandra.ISession _session) : IFunkoPriceHistoryRepository
{
    private bool _tableCreated = false;

    private async Task EnsureTableCreatedAsync()
    {
        if (_tableCreated) return;

        var createTableCql = @"
            CREATE TABLE IF NOT EXISTS historial_precios_por_funko (
                funko_id bigint,
                fecha timestamp,
                precio decimal,
                precio_anterior decimal,
                PRIMARY KEY (funko_id, fecha)
            ) WITH CLUSTERING ORDER BY (fecha DESC);";

        await _session.ExecuteAsync(new SimpleStatement(createTableCql));
        _tableCreated = true;
    }

    public async Task InsertarHistorialAsync(long funkoId, decimal precioNuevo, decimal precioAnterior)
    {
        await EnsureTableCreatedAsync();

        var insertCql = @"
            INSERT INTO historial_precios_por_funko (funko_id, fecha, precio, precio_anterior)
            VALUES (?, ?, ?, ?);";

        var prepared = await _session.PrepareAsync(insertCql);
        var statement = prepared.Bind(funkoId, DateTimeOffset.UtcNow, precioNuevo, precioAnterior);

        await _session.ExecuteAsync(statement);
    }

    public async Task<IEnumerable<HistorialPrecioFunkoModel>> ObtenerHistorialPorFunkoAsync(long funkoId)
    {
        await EnsureTableCreatedAsync();

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
        });
    }
}