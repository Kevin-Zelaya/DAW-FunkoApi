using Microsoft.Extensions.Options;
using Serilog;

public static class SignalRConfig
{
    public static IServiceCollection AddSignalRconfiguration(
        this IServiceCollection services
    )
    {
        Log.Information("Configurando signalR...");
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(30);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
        });
        Log.Information("SignalR configurado correctamente.");
        return services;
    }
}