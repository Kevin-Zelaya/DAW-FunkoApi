using Cassandra;
using Microsoft.Extensions.Options;
using MinimalApi.config;
using MinimalApi.repositories;

namespace MinimalApi.extensions;

public static class CassandraServiceCollectionExtensions
{
    public static IServiceCollection AddCassandra(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CassandraSettings>(configuration.GetSection("DbConnections:Cassandra"));

        services.AddSingleton<Cassandra.ISession>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CassandraSettings>>().Value;

            var cluster = Cluster.Builder()
                .AddContactPoints(settings.ContactPoints)
                .WithPort(settings.Port)
                .Build();

            using (var tempSession = cluster.Connect())
            {
                tempSession.Execute($@"
                    CREATE KEYSPACE IF NOT EXISTS {settings.Keyspace} 
                    WITH replication = {{ 'class': 'SimpleStrategy', 'replication_factor': '1' }};");
            }

            return cluster.Connect(settings.Keyspace);
        });

        services.AddScoped<IFunkoPriceHistoryRepository, FunkoPriceHistoryRepository>();

        return services;
    }
}