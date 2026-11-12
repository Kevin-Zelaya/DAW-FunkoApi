using MinimalApi.repositories;
using Serilog;

public static class RepositoriesConfig
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        Log.Information("Registrando repositorios...");
        services.AddScoped<IFunkoRepository, FunkoRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISupplierRepository, SuppliersRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFunkoPriceHistoryRepository, FunkoPriceHistoryRepository>();
        Log.Information("Repositorios registrados correctamente.");
        return services;
    }
}