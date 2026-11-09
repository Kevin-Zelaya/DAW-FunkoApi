using MongoDB.Driver;
using Serilog;
using StackExchange.Redis;

public static class InitializationDb
{
    // Inicialización normal: conservar datos
    public static async Task InitializeDatabasesAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var mongoContext = scope.ServiceProvider
            .GetRequiredService<AppDbMongoContext>();

        // PostgreSQL
        await context.Database.EnsureCreatedAsync();

        // MongoDB
        await mongoContext.Database.EnsureCreatedAsync();

        Log.Information("[DATABASES] Bases de datos inicializadas.");
    }

    // Reconstrucción: eliminar datos
    public static async Task RecreateDatabasesAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var mongoContext = scope.ServiceProvider
            .GetRequiredService<AppDbMongoContext>();

        var redis = scope.ServiceProvider
            .GetRequiredService<IConnectionMultiplexer>();

        var server = redis.GetServer(redis.GetEndPoints().First());

        // PostgreSQL
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        Log.Information("[POSTGRES] Base de datos recreada.");

        // MongoDB
        await mongoContext.Database.EnsureDeletedAsync();
        await mongoContext.Database.EnsureCreatedAsync();

        Log.Information("[MONGO] Base de datos recreada.");

        // Redis
        await server.FlushAllDatabasesAsync();

        Log.Information("[REDIS] Caché vaciada");
    }
}