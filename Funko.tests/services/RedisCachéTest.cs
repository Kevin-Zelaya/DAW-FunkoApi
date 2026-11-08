
using NUnit.Framework;
using StackExchange.Redis;
using Testcontainers.Redis;

[TestFixture]
public class RedisCacheTests
{
    private RedisContainer _container = null!;
    private IConnectionMultiplexer _redis = null!;
    private RedisCache _cache = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();

        await _container.StartAsync();

        _redis = await ConnectionMultiplexer.ConnectAsync(
            _container.GetConnectionString()
        );
    }

    [SetUp]
    public async Task Setup()
    {
        _cache = new RedisCache(_redis);

        await _cache.ClearAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _redis.DisposeAsync();
        await _container.DisposeAsync();
    }

    public class CacheTestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public CacheTestDto CreateCacheTestDto() => new CacheTestDto
    {
        Id = 1,
        Name = "Proveedor de prueba"
    };

    // =========================================================
    // SET
    // =========================================================

    [Test]
    public async Task SetAsync_ValidString_ValueStored()
    {
        // Arrange
        string key = "TEST:STRING";
        string value = "Hola Redis";

        // Act
        await _cache.SetAsync(key, value);

        var response = await _cache.GetTAsync<string>(key);

        // Assert
        Assert.That(response, Is.EqualTo(value));
    }

    [Test]
    public async Task SetAsync_ValidObject_ObjectStored()
    {
        // Arrange
        string key = "TEST:OBJECT";
        var value = CreateCacheTestDto();

        // Act
        await _cache.SetAsync(key, value);

        var response = await _cache.GetTAsync<CacheTestDto>(key);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Id, Is.EqualTo(value.Id));
        Assert.That(response.Name, Is.EqualTo(value.Name));
    }

    [Test]
    public async Task SetAsync_ExistingKey_ValueUpdated()
    {
        // Arrange
        string key = "TEST:UPDATE";

        await _cache.SetAsync(key, "Valor original");

        // Act
        await _cache.SetAsync(key, "Valor actualizado");

        var response = await _cache.GetTAsync<string>(key);

        // Assert
        Assert.That(response, Is.EqualTo("Valor actualizado"));
    }

    [Test]
    public async Task SetAsync_WithoutExpiration_DefaultExpiration()
    {
        // Arrange
        string key = "TEST:DEFAULT-EXPIRATION";

        // Act
        await _cache.SetAsync(key, "Valor");

        var expiration = await _redis
            .GetDatabase()
            .KeyTimeToLiveAsync(key);

        // Assert
        Assert.That(expiration, Is.Not.Null);
        Assert.That(expiration!.Value.TotalMinutes,
            Is.InRange(9, 10));
    }

    [Test]
    public async Task SetAsync_CustomExpiration_ExpirationConfigured()
    {
        // Arrange
        string key = "TEST:CUSTOM-EXPIRATION";
        var expiration = TimeSpan.FromSeconds(30);

        // Act
        await _cache.SetAsync(key, "Valor", expiration);

        var ttl = await _redis
            .GetDatabase()
            .KeyTimeToLiveAsync(key);

        // Assert
        Assert.That(ttl, Is.Not.Null);
        Assert.That(ttl!.Value.TotalSeconds,
            Is.InRange(0, 30));
    }

    // =========================================================
    // GET
    // =========================================================

    [Test]
    public async Task GetTAsync_ExistingKey_ValueReturned()
    {
        // Arrange
        string key = "TEST:GET";

        await _cache.SetAsync(key, "Valor guardado");

        // Act
        var response = await _cache.GetTAsync<string>(key);

        // Assert
        Assert.That(response, Is.EqualTo("Valor guardado"));
    }

    [Test]
    public async Task GetTAsync_NonExistingKey_NullReturned()
    {
        // Arrange
        string key = "TEST:NON-EXISTING";

        // Act
        var response = await _cache.GetTAsync<string>(key);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task GetTAsync_ExpiredKey_NullReturned()
    {
        // Arrange
        string key = "TEST:EXPIRED";

        await _cache.SetAsync(
            key,
            "Valor temporal",
            TimeSpan.FromMilliseconds(100)
        );

        // Act
        await Task.Delay(300);

        var response = await _cache.GetTAsync<string>(key);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task GetTAsync_InvalidJson_JsonException()
    {
        // Arrange
        string key = "TEST:INVALID-JSON";

        await _redis.GetDatabase()
            .StringSetAsync(key, "esto no es json");

        // Act
        Assert.ThrowsAsync<System.Text.Json.JsonException>(async () =>
            await _cache.GetTAsync<CacheTestDto>(key)
        );

        // Assert
    }

    // =========================================================
    // REMOVE
    // =========================================================

    [Test]
    public async Task RemoveAsync_ExistingKey_KeyRemoved()
    {
        // Arrange
        string key = "TEST:REMOVE";

        await _cache.SetAsync(key, "Valor");

        // Act
        await _cache.RemoveAsync(key);

        var response = await _cache.GetTAsync<string>(key);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task RemoveAsync_NonExistingKey_NoException()
    {
        // Arrange
        string key = "TEST:REMOVE-NON-EXISTING";

        // Act
        await _cache.RemoveAsync(key);

        // Assert
        var response = await _cache.GetTAsync<string>(key);

        Assert.That(response, Is.Null);
    }

    // =========================================================
    // CLEAR
    // =========================================================

    [Test]
    public async Task ClearAsync_ExistingKeys_AllKeysRemoved()
    {
        // Arrange
        await _cache.SetAsync("TEST:KEY1", "Valor 1");
        await _cache.SetAsync("TEST:KEY2", "Valor 2");
        await _cache.SetAsync("TEST:KEY3", "Valor 3");

        // Act
        await _cache.ClearAsync();

        // Assert
        Assert.That(
            await _cache.GetTAsync<string>("TEST:KEY1"),
            Is.Null
        );

        Assert.That(
            await _cache.GetTAsync<string>("TEST:KEY2"),
            Is.Null
        );

        Assert.That(
            await _cache.GetTAsync<string>("TEST:KEY3"),
            Is.Null
        );
    }

    [Test]
    public async Task ClearAsync_EmptyCache_NoException()
    {
        // Arrange
        // Act
        await _cache.ClearAsync();

        // Assert
        var keys = _redis
            .GetServer(_redis.GetEndPoints().First())
            .Keys(pattern: "*")
            .ToList();

        Assert.That(keys, Is.Empty);
    }
}
