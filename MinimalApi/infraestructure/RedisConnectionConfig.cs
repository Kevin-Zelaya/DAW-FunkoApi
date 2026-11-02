using Serilog;
using StackExchange.Redis;

public static class RedisConnectionConfig
{
    public static IServiceCollection AddRedisConnection(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        Log.Information("Configurando la conexión a Redis...");
        var redisConfig = configuration
            .GetSection("Redis")
            .Get<RedisConfig>();

        var redis = ConnectionMultiplexer.Connect(
            AppConfig.RedisConnection
        );
        services.AddSingleton<IConnectionMultiplexer>(redis);
        Log.Information("Conexión a Redis configurada correctamente.");
        return services;
    }
    
}