using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;

[Authorize]
public class FunkoHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = Context.User?.FindFirstValue(ClaimTypes.Email);
        var isAdmin = Context.User?.IsInRole("ADMIN") ?? false;

        Log.Information("Usuario conectado: {Email} (ID: {UserId})", email, userId);
        if (isAdmin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            Log.Debug("Usuario {UserId} agregado a grupo admins", userId);
        }
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        Log.Information("Usuario desconectado: {UserId}", userId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinCategoriaGroup(long categoriaId)
    {
        var groupName = $"categoria-{categoriaId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        Log.Debug("Conexion {ConnectionId} unida a grupo {Group}", Context.ConnectionId, groupName);

        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    /// <summary>
    /// Salir de un grupo de categoria.
    /// </summary>
    public async Task LeaveCategoriaGroup(long categoriaId)
    {
        var groupName = $"categoria-{categoriaId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        Log.Debug("Conexion {ConnectionId} eliminada del grupo {Group}", Context.ConnectionId, groupName);

        await Clients.Caller.SendAsync("LeftGroup", groupName);
    }
}