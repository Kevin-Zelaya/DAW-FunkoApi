using Cassandra;
using MinimalApi.config;
using Serilog;
using ISession = Cassandra.ISession;

public static class CassandraConfig
{
    public static IServiceCollection AddCassandra(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // cargamos la configuración desde los appsettigns
        // si no se crea un nuevo objeto que tiene los valores
        // por defecto.
        var settings = configuration
            .GetSection(CassandraSettings.SectionName)
            .Get<CassandraSettings>() ?? new CassandraSettings();
        
        Log.Information("Conectando a cassandra: {Host}:{Port}",
        settings.Hosts, settings.Port);

        var cluster =  Cluster.Builder()
            .AddContactPoint(settings.Hosts)
            .WithPort(settings.Port)
            .Build();

        ISession session;
        for (var intento = 1; ; intento++)
        {
            try
            {
                session = cluster.Connect();
                break;
            }
            catch (NoHostAvailableException) when (intento < 30)
            {
                Log.Information("Cassandra aun no acepta conexiones (intento {Intento}/30)...", intento);
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
        // configuración de cassandra, vive durante toda la ejecución
        services.AddSingleton(settings);
        services.AddSingleton<ISession>(session);
        return services;
    }
}