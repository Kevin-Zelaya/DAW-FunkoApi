using System.Security.Cryptography.X509Certificates;
using Cassandra;
using Serilog;
using ISession = Cassandra.ISession;

public static class CassandraSchemaExtensions
{
    public static async Task InitializeKeyspaceAsync(
        this ISession session,
        string keyspace)
    {
        // Esperar a que Cassandra esté disponible
        for (var intento = 1; ; intento++)
        {
            try
            {
                await session.ExecuteAsync(
                    new SimpleStatement("SELECT now() FROM system.local"));

                break;
            }
            catch (NoHostAvailableException) when (intento < 30)
            {
                Log.Information(
                    "Cassandra aún no acepta conexiones (intento {Intento}/30)...",
                    intento);

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        // Crear keyspace si no existe
        await session.ExecuteAsync(new SimpleStatement(
            $"CREATE KEYSPACE IF NOT EXISTS {keyspace} " +
            "WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1}"));

        // Seleccionar keyspace
        session.ChangeKeyspace(keyspace);

        Log.Information(
            "Conectado al keyspace {Keyspace}",
            keyspace);
    }

    public static async Task InitializeTablesAsync(
        this ISession session,
        bool shouldRecreate)
    {
        if (shouldRecreate)
        {
            Log.Information("[CASSANDRA] Eliminando tablas existentes.");

            await session.ExecuteAsync(new SimpleStatement(
                "DROP TABLE IF EXISTS funko_price_history"));
        }

        await session.ExecuteAsync(new SimpleStatement(@"
            CREATE TABLE IF NOT EXISTS funko_price_history (
                funko_id bigint,
                date timestamp,
                price decimal,
                old_price decimal,
                deleted BOOLEAN,
                PRIMARY KEY (funko_id, date)
            ) WITH CLUSTERING ORDER BY (date DESC);"));

        Log.Information("[CASSANDRA] Tablas inicializadas.");
    }
}