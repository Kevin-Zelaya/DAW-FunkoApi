using Microsoft.AspNetCore.Authentication;
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
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        Log.Information("Servicios configurados correctamente.");
        return services;
    }
}