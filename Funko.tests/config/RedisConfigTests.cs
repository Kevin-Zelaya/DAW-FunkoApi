
[TestFixture]
public class RedisConfigTests
{
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
}
