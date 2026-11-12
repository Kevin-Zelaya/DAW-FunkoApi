namespace MinimalApi.config;

public class CassandraSettings
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "DbConnections:Cassandra";

    /// <summary>Nodos de contacto del cluster.</summary>
    public string[] ContactPoints { get; set; } = ["localhost"];

    /// <summary>Puerto del protocolo CQL.</summary>
    public int Port { get; set; } = 9042;

    /// <summary>Keyspace de la aplicación.</summary>
    public string Keyspace { get; set; } = "funkos_store";
}