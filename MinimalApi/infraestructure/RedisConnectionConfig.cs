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

        var options = ConfigurationOptions.Parse(
            redisConfig!.ConnectionString
        );

        options.AllowAdmin = true;

        var redis = ConnectionMultiplexer.Connect(options);

        services.AddSingleton<IConnectionMultiplexer>(redis);

        Log.Information("Conexión a Redis configurada correctamente.");

        return services;
    }
    
}