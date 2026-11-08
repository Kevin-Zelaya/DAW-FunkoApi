using Microsoft.AspNetCore.SignalR;
using Serilog;

public class NotificationService(IHubContext<FunkoHub> _hubContext) : INotificationService
{
    /// <inheritdoc cref="INotificationService.NotifyResourceChangedAsync{T}(ResourceNotification{T})"/>
    public async Task NotifyResourceChangedAsync<T>(
        ResourceNotification<T> notification)
    {
        try
        {
            await _hubContext.Clients
                .Group("admins")
                .SendAsync("ResourceUpdated", notification);

            Log.Information("Notification sent: {Resource} updated successfully", notification.resource
            );
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error al enviar notificacion de producto eliminado");
        }
    }
}