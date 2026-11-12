using Serilog;

public static class StorageConfig
{
    public static IServiceCollection AddStorage(
        this IServiceCollection services
    )
    {
        Log.Information("Configurando el almacenamiento de archivos...");
        services.AddSingleton<IStorageService, FileSystemStorageService>();
        Log.Information("Almacenamiento de archivos configurado correctamente.");
        return services;
    }
}