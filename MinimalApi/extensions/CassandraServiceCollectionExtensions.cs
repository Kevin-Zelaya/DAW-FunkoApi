using Cassandra;
using Microsoft.Extensions.Options;
// Asegúrate de que estos namespaces coincidan exactamente con los que tienes en tus archivos de configuración y repositorios:
using MinimalApi.configuration; // O MinimalApi.Configuration (según cómo esté escrito en tu clase CassandraSettings)
using MinimalApi.repositories; 

namespace MinimalApi.extensions;

public static class CassandraServiceCollectionExtensions
{
    public static IServiceCollection AddCassandra(this IServiceCollection services, IConfiguration configuration)
    {
        // Enlazar la configuración tipada desde appsettings.json
        services.Configure<CassandraSettings>(configuration.GetSection("DbConnections:Cassandra"));

        // Registrar la sesión de Cassandra (ISession) como Singleton de forma explícita
        services.AddSingleton<Cassandra.ISession>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CassandraSettings>>().Value;

            // Conectar al clúster usando los ContactPoints y el puerto del appsettings
            var cluster = Cluster.Builder()
                .AddContactPoints(settings.ContactPoints)
                .WithPort(settings.Port)
                .Build();

            // Crear el keyspace si no existe antes de conectar
            using (var tempSession = cluster.Connect())
            {
                tempSession.Execute($@"
                    CREATE KEYSPACE IF NOT EXISTS {settings.Keyspace} 
                    WITH replication = {{ 'class': 'SimpleStrategy', 'replication_factor': '1' }};");
            }

            // Conectar definitivamente al keyspace configurado
            return cluster.Connect(settings.Keyspace);
        });

        // Registrar el repositorio de Cassandra
        services.AddScoped<IFunkoPriceHistoryRepository, FunkoPriceHistoryRepository>();

        return services;
    }
}