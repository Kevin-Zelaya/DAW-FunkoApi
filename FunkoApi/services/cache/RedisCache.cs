
using System.Text.Json;
using StackExchange.Redis;

public class RedisCache(IConnectionMultiplexer redis) : ICache, IScoped
{
    private readonly IDatabase _db = redis.GetDatabase();

    /// <inheritdoc cref="ICache.GetTAsync{T}(string)"/>
    public async Task<T?> GetTAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>((string)value!);
    }

    /// <inheritdoc cref="ICache.SetAsync{T}(string, T, TimeSpan?)"/>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry: expiration ?? TimeSpan.FromMinutes(10));
    }

    /// <inheritdoc cref="ICache.RemoveAsync(string)"/>
    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    /// <inheritdoc cref="ICache.ClearAsync()"/>
    public async Task ClearAsync()
    {
        var endpoints = redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = redis.GetServer(endpoint);
            if (!server.IsConnected)
            {
                continue;
            }

            var keys = server.Keys(pattern: "*").ToArray();
            if (keys.Length == 0)
            {
                continue;
            }

            await _db.KeyDeleteAsync(keys);
        }
    }
}