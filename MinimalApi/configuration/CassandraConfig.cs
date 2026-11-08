namespace MinimalApi.config;

public class CassandraSettings
{
    /// <summary>Nombre de la seccion en appsettings.json.</summary>
    public const string SectionName = "DbConnections:Cassandra";
    /// <summary>Nodos de contacto del cluster (los clientes hablan por el puerto 9043).</summary>
    public string Hosts { get; set; } = "localhost";

    /// <summary>Puerto del protocolo CQL.</summary>
    public int Port { get; set; } = 9042;

    /// <summary>Keyspace de la aplicacion (el equivalente a una base de datos).</summary>
    public string Keyspace { get; set; } = "funkos_store";
}