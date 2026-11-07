using MongoDB.Driver;
using Serilog;
using StackExchange.Redis;

public static class InitializationDb
{
    public static async Task InitializationDatabaseAsync(
        this WebApplication app)
    {
        
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mongoContext = scope.ServiceProvider.GetRequiredService<AppDbMongoContext>();
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var server = redis.GetServer(redis.GetEndPoints().First());
        ///<summary> Eliminar base de datos <summary>
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        Log.Information("[POSGRE] Eliminando y recreando la base de datos");
        await mongoContext.Database.EnsureDeletedAsync();
        await mongoContext.Database.EnsureCreatedAsync();
        Log.Information("[MONGO] Eliminando y recreando la base de datos");
        await server.FlushAllDatabasesAsync(); // vaciar caché
        Log.Information("[REDIS] Vaciando la caché");

    }
}