using System.Security.Cryptography.X509Certificates;
using Cassandra;
using Serilog;
using ISession = Cassandra.ISession;

public static class CassandraSchemaExtensions
{
    public static async Task InitializeSchemaAsync(
        this ISession session,
        string keyspace
    )
    {
        for (var intento = 1; ; intento++)
        {
            try
            {
                await session.ExecuteAsync(new SimpleStatement("SELECT now() FROM system.local"));
                break;
            }
            catch (NoHostAvailableException) when (intento < 30)
            {
                Log.Information("Cassandra aun no acepta conexiones (intento {Intento}/30)...", intento);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        await session.ExecuteAsync(new SimpleStatement(
            $"CREATE KEYSPACE IF NOT EXISTS {keyspace} " +
            "WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1}"));

        session.ChangeKeyspace(keyspace);
        Log.Information("[CASSANDRA] Eliminando y recreando las tablas.");
        // Eliminar la tabla existente: se puede separar para futuras consultas
        await session.ExecuteAsync(new SimpleStatement(
            "DROP TABLE IF EXISTS funko_price_history"));
        // Tabla de precios
        await session.ExecuteAsync(new SimpleStatement(@"
            CREATE TABLE IF NOT EXISTS funko_price_history (
                funko_id bigint,
                date timestamp,
                price decimal,
                old_price decimal,
                deleted BOOLEAN,
                PRIMARY KEY (funko_id, date)
            ) WITH CLUSTERING ORDER BY (date DESC);"));

        Log.Information("Esquema de Cassandra listo en el keyspace {Keyspace}", keyspace);
    }
}