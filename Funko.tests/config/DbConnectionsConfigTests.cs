
[TestFixture]
public class DbConnectionsConfigTests
{
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
}
