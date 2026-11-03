namespace MinimalApi.config;

public class CassandraSettings
{
    public string[] ContactPoints { get; set; } = [];
    public int Port { get; set; } = 9042;
    public string Keyspace { get; set; } = "funko_store";
}