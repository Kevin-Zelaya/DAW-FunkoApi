public class DbConnectionsConfig
{
    public DatabaseProviderConfig Postgre { get; set; } = new();
    public DatabaseProviderConfig Mongo { get; set; } = new();
}

public class DatabaseProviderConfig
{
    public ConnectionStringsConfig ConnectionStrings { get; set; } = new();
}

public class ConnectionStringsConfig
{
    public string DefaultConnection { get; set; } = string.Empty;
}