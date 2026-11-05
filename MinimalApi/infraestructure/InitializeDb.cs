using MongoDB.Driver;
using Serilog;

public static class InitializationDb
{
    public static async Task InitializationDatabaseAsync(
        this WebApplication app)
    {
        
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mongoContext = scope.ServiceProvider.GetRequiredService<AppDbMongoContext>();

        ///<summary> Eliminar base de datos <summary>
        Log.Debug($"[{app.Environment.ToString()!.ToUpper()}] Eliminando y recreando la base de datos Postgre");
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        Log.Information("[POSGRE] Eliminando y recreando la base de datos");
        Log.Debug($"[{app.Environment.ToString()!.ToUpper()}] Eliminando y recreando la base de datos Mongo");
        await mongoContext.Database.EnsureDeletedAsync();
        await mongoContext.Database.EnsureCreatedAsync();
        Log.Information("[POSGRE] Eliminando y recreando la base de datos");
    }
}