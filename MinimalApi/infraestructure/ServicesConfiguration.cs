using Serilog;

public static class ServicesConfiguration
{
    public static IServiceCollection AddServices(
        this IServiceCollection services)
    {
        Log.Information("Configurando servicios...");
        services.AddScoped<IFunkoService, FunkoService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICache, RedisCache>();
        Log.Information("Servicios configurados correctamente.");
        return services;
    }
}