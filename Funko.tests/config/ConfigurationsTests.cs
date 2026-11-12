
using MinimalApi.config;
using MinimalApi.Services.Email;

[TestFixture]
public class ConfigurationTests
{
    // =========================================================
    // API CONFIG
    // =========================================================

    [Test]
    public void ApiConfig_Initialize_DefaultValues()
    {
        // Arrange
        var config = new ApiConfig();

        // Assert
        Assert.That(config.Nombre, Is.EqualTo("Productos EF Core API"));
        Assert.That(
            config.Mensaje,
            Is.EqualTo("CRUD con Entity Framework Core + PostgreSQL")
        );
        Assert.That(config.shouldRecreateDb, Is.False);
    }

    [Test]
    public void ApiConfig_SetProperties_ValuesUpdated()
    {
        // Arrange
        var config = new ApiConfig();

        // Act
        config.Nombre = "Funko API";
        config.Mensaje = "API de gestión de Funkos";
        config.shouldRecreateDb = true;

        // Assert
        Assert.That(config.Nombre, Is.EqualTo("Funko API"));
        Assert.That(config.Mensaje, Is.EqualTo("API de gestión de Funkos"));
        Assert.That(config.shouldRecreateDb, Is.True);
    }

    // =========================================================
    // CASSANDRA SETTINGS
    // =========================================================

    [Test]
    public void CassandraSettings_Initialize_DefaultValues()
    {
        // Arrange
        var config = new CassandraSettings();

        // Assert
        Assert.That(config.ContactPoints, Is.EqualTo(new[] { "localhost" }));
        Assert.That(config.Port, Is.EqualTo(9042));
        Assert.That(config.Keyspace, Is.EqualTo("funkos_store"));
        Assert.That(
            CassandraSettings.SectionName,
            Is.EqualTo("DbConnections:Cassandra")
        );
    }

    [Test]
    public void CassandraSettings_SetProperties_ValuesUpdated()
    {
        // Arrange
        var config = new CassandraSettings();

        // Act
        config.ContactPoints = ["192.168.1.10", "192.168.1.11"];
        config.Port = 9142;
        config.Keyspace = "test_keyspace";

        // Assert
        Assert.That(
            config.ContactPoints,
            Is.EqualTo(new[] { "192.168.1.10", "192.168.1.11" })
        );
        Assert.That(config.Port, Is.EqualTo(9142));
        Assert.That(config.Keyspace, Is.EqualTo("test_keyspace"));
    }

    // =========================================================
    // DATABASE CONNECTIONS CONFIG
    // =========================================================

    [Test]
    public void DbConnectionsConfig_Initialize_PropertiesInitialized()
    {
        // Arrange
        var config = new DbConnectionsConfig();

        // Assert
        Assert.That(config.Postgre, Is.Not.Null);
        Assert.That(config.Mongo, Is.Not.Null);
    }

    [Test]
    public void DatabaseProviderConfig_Initialize_ConnectionStringsInitialized()
    {
        // Arrange
        var config = new DatabaseProviderConfig();

        // Assert
        Assert.That(config.ConnectionStrings, Is.Not.Null);
    }

    [Test]
    public void ConnectionStringsConfig_Initialize_DefaultConnection()
    {
        // Arrange
        var config = new ConnectionStringsConfig();

        // Assert
        Assert.That(config.DefaultConnection, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ConnectionStringsConfig_SetDefaultConnection_ValueUpdated()
    {
        // Arrange
        var config = new ConnectionStringsConfig();

        // Act
        config.DefaultConnection = "Host=localhost;Database=Funkos";

        // Assert
        Assert.That(
            config.DefaultConnection,
            Is.EqualTo("Host=localhost;Database=Funkos")
        );
    }

    // =========================================================
    // REDIS CONFIG
    // =========================================================

    [Test]
    public void RedisConfig_Initialize_DefaultConnectionString()
    {
        // Arrange
        var config = new RedisConfig();

        // Assert
        Assert.That(config.ConnectionString, Is.EqualTo(string.Empty));
    }

    [Test]
    public void RedisConfig_SetConnectionString_ValueUpdated()
    {
        // Arrange
        var config = new RedisConfig();

        // Act
        config.ConnectionString = "localhost:6379";

        // Assert
        Assert.That(config.ConnectionString, Is.EqualTo("localhost:6379"));
    }

    // =========================================================
    // SMTP SETTINGS
    // =========================================================

    [Test]
    public void SmtpSettings_Initialize_DefaultValues()
    {
        // Arrange
        var config = new SmtpSettings();

        // Assert
        Assert.That(config.Host, Is.EqualTo("localhost"));
        Assert.That(config.Port, Is.EqualTo(587));
        Assert.That(config.Username, Is.Null);
        Assert.That(config.Password, Is.Null);
        Assert.That(config.FromEmail, Is.EqualTo("noreply@funkoapi.com"));
        Assert.That(config.FromName, Is.EqualTo("FunkoApp"));
        Assert.That(config.AdminEmail, Is.EqualTo("admin@funkoapi.com"));
        Assert.That(config.Security, Is.EqualTo("Auto"));
    }

    [Test]
    public void SmtpSettings_SetProperties_ValuesUpdated()
    {
        // Arrange
        var config = new SmtpSettings();

        // Act
        config.Host = "smtp.gmail.com";
        config.Port = 465;
        config.Username = "test@gmail.com";
        config.Password = "password";
        config.FromEmail = "sender@gmail.com";
        config.FromName = "Test App";
        config.AdminEmail = "admin@gmail.com";
        config.Security = "SslOnConnect";

        // Assert
        Assert.That(config.Host, Is.EqualTo("smtp.gmail.com"));
        Assert.That(config.Port, Is.EqualTo(465));
        Assert.That(config.Username, Is.EqualTo("test@gmail.com"));
        Assert.That(config.Password, Is.EqualTo("password"));
        Assert.That(config.FromEmail, Is.EqualTo("sender@gmail.com"));
        Assert.That(config.FromName, Is.EqualTo("Test App"));
        Assert.That(config.AdminEmail, Is.EqualTo("admin@gmail.com"));
        Assert.That(config.Security, Is.EqualTo("SslOnConnect"));
    }
}
