using Cassandra;
using MinimalApi.Models;

namespace MinimalApi.repositories;

public class FunkoPriceHistoryRepository : IFunkoPriceHistoryRepository
{
    private readonly ISession _session;

    public FunkoPriceHistoryRepository(ISession session)
    {
        _session = session;
        InicializarTabla();
    }

    private void InicializarTabla()
    {
        var query = @"
            CREATE TABLE IF NOT EXISTS historial_precios_por_funko (
                funko_id bigint,
                fecha timestamp,
                precio double,
                precio_anterior double,
                PRIMARY KEY (funko_id, fecha)
            ) WITH CLUSTERING ORDER BY (fecha DESC);";
        
        _session.Execute(query);
    }

    public async Task InsertarHistorialAsync(long funkoId, double precio, double precioAnterior)
    {
        var query = "INSERT INTO historial_precios_por_funko (funko_id, fecha, precio, precio_anterior) VALUES (?, ?, ?, ?)";
        var prepared = await _session.PrepareAsync(query);
        var statement = prepared.Bind(funkoId, DateTime.UtcNow, precio, precioAnterior);
        
        await _session.ExecuteAsync(statement);
    }

    public async Task<IEnumerable<HistorialPrecioFunkoModel>> ObtenerHistorialPorFunkoAsync(long funkoId)
    {
        var query = "SELECT funko_id, fecha, precio, precio_anterior FROM historial_precios_por_funko WHERE funko_id = ?";
        var prepared = await _session.PrepareAsync(query);
        var statement = prepared.Bind(funkoId);
        
        var resultSet = await _session.ExecuteAsync(statement);

        return resultSet.Select(row => new HistorialPrecioFunkoModel
        {
            FunkoId = row.GetValue<long>("funko_id"),
            Fecha = row.GetValue<DateTime>("fecha"),
            Precio = row.GetValue<double>("precio"),
            PrecioAnterior = row.GetValue<double>("precio_anterior")
        });
    }
}