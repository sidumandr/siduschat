using System.Security.Claims;
using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Hubs;

[Authorize]
public class ChatHub(
    IChatService    chatService,
    IRoomService    roomService,
    IPresenceService presence) : Hub
{
    // when connected
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        
        await presence.SetOnlineAsync(userId, Context.ConnectionId);

        var rooms = await roomService.GetUserRoomsAsync(userId);
        foreach (var room in rooms)
            await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(room.Id));

        foreach (var room in rooms)
            await Clients.Group(RoomGroup(room.Id))
                         .SendAsync("presence:online", new { userId });

        await base.OnConnectedAsync();
    }

    // when disconnected
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        await presence.SetOfflineAsync(Context.ConnectionId);

        var rooms = await roomService.GetUserRoomsAsync(userId);
        foreach (var room in rooms)
            await Clients.Group(RoomGroup(room.Id))
                         .SendAsync("presence:offline", new { userId });

        await base.OnDisconnectedAsync(exception);
    }

    // Client to Server: send message
    // Frontend: connection.invoke("SendMessage", { roomId, content })
    public async Task SendMessage(SendMessageRequest request)
    {
        var senderId = GetUserId();
        await chatService.SendMessageAsync(senderId, request);
    }

    // ── Client to Server: typing
    // Frontend: connection.invoke("Typing", roomId, true/false)
    public async Task Typing(Guid roomId, bool isTyping)
    {
        var userId = GetUserId();

        // OthersInGroup
        await Clients.OthersInGroup(RoomGroup(roomId))
                     .SendAsync("user:typing", new { roomId, userId, isTyping });
    }

    private Guid GetUserId()
    {
        var value = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? Context.User!.FindFirstValue("sub")!;
        return Guid.Parse(value);
    }

    private static string RoomGroup(Guid roomId) => $"room:{roomId}";
}

// IChatNotifier entg
public class SignalRChatNotifier(IHubContext<ChatHub> hub) : IChatNotifier
{
    private static string RoomGroup(Guid roomId) => $"room:{roomId}";

    public Task NotifyRoomAsync(Guid roomId, string @event, object payload) =>
        hub.Clients.Group(RoomGroup(roomId)).SendAsync(@event, payload);

    public Task NotifyUserAsync(Guid userId, string @event, object payload) =>
        hub.Clients.User(userId.ToString()).SendAsync(@event, payload);
}