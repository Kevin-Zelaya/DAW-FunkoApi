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
        services.AddSingleton(settings);

        Log.Information("Conectando a cassandra: {Host}:{Port}",
        settings.Hosts, settings.Port);

        var cluster =  Cluster.Builder()
            .AddContactPoint(settings.Hosts)
            .WithPort(settings.Port)
            .Build();

        var session = cluster.Connect();

        services.AddSingleton<ISession>(session);
        services.AddSingleton(cluster);

        return services;
    }
}