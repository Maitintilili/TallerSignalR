using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalR.HubServer.Data;
using SignalR.Shared.Contracts;

namespace SignalR.HubServer.Hubs;

[Authorize]
public sealed class ChatHub(MessageRepository messages) : Microsoft.AspNetCore.SignalR.Hub
{
    // connectionId -> usuario (para la lista de "En línea")
    private static readonly ConcurrentDictionary<string, string> Online = new();

    private long CurrentUserId =>
        long.Parse((Context.User!.FindFirst("sub") ?? Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier))!.Value);

    private string CurrentUser => Context.User!.Identity!.Name!;

    public override async Task OnConnectedAsync()
    {
        Online[Context.ConnectionId] = CurrentUser;

        // El que se conecta recibe el historial guardado en la tabla Mensajes
        await Clients.Caller.SendAsync("LoadHistory", await messages.GetLastAsync());
        await BroadcastOnlineAsync();

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Online.TryRemove(Context.ConnectionId, out _);
        await BroadcastOnlineAsync();

        await base.OnDisconnectedAsync(exception);
    }

    // El usuario ya NO viene del cliente: se toma del token, así nadie puede escribir como otra persona.
    public async Task SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new HubException("El mensaje es obligatorio.");

        message = message.Trim();

        if (message.Length > 1000)
            throw new HubException("El mensaje supera el límite permitido.");

        var sentAt = DateTimeOffset.UtcNow;
        await messages.AddAsync(CurrentUserId, message, sentAt);

        var chatMessage = new ChatMessage(CurrentUser, message, sentAt);
        await Clients.All.SendAsync("ReceiveMessage", chatMessage);
    }

    private Task BroadcastOnlineAsync() =>
        Clients.All.SendAsync("OnlineUsers", Online.Values.Distinct().Order().ToList());
}
