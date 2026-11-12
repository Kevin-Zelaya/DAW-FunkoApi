using Microsoft.EntityFrameworkCore;
using Serilog;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabases(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Information("Configurando la base de datos...");
        var dbConfig = configuration
            .GetSection("DbConnections")
            .Get<DbConnectionsConfig>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dbConfig!.Postgre.ConnectionStrings.DefaultConnection));
        services.AddDbContext<AppDbMongoContext>(options =>
            options.UseMongoDB(dbConfig!.Mongo.ConnectionStrings.DefaultConnection));
        Log.Information("Bases de datos configurada correctamente.");
        return services;
    }
}