using Cassandra;
using MinimalApi.config;
using Serilog;
using ISession = Cassandra.ISession;

public static class CassandraConfig
{
    public static IServiceCollection AddCassandra(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(CassandraSettings.SectionName)
            .Get<CassandraSettings>() ?? new CassandraSettings();

        services.AddSingleton(settings);

        Log.Information("Conectando a cassandra: {Hosts}:{Port}",
            string.Join(", ", settings.ContactPoints), settings.Port);

        var cluster = Cluster.Builder()
            .AddContactPoints(settings.ContactPoints)
            .WithPort(settings.Port)
            .Build();

        var session = cluster.Connect();

        services.AddSingleton<ISession>(session);
        services.AddSingleton(cluster);

        return services;
    }
}