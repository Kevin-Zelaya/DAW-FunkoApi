namespace MinimalApi.configuration;

public class CassandraSettings
{
    public string[] ContactPoints { get; set; } = new[] { "localhost" };
    public int Port { get; set; } = 9042;
    public string Keyspace { get; set; } = "funko_store";
}