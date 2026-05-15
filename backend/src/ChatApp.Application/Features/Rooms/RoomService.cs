using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using ChatApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Features.Rooms;

public class RoomService(
    IUnitOfWork          uow,
    IChatNotifier        notifier,
    ILogger<RoomService> logger) : IRoomService
{
    public async Task<RoomDto> CreateRoomAsync(Guid createdByUserId, CreateRoomRequest req, CancellationToken ct = default)
    {
        var type = Enum.Parse<RoomType>(req.Type, ignoreCase: true);
        var room = type == RoomType.Private
            ? Room.CreatePrivate(req.Name, createdByUserId)
            : Room.CreatePublic(req.Name, createdByUserId, req.Description);

        await uow.Rooms.AddAsync(room, ct);

        var membership = RoomMember.Create(room.Id, createdByUserId, "owner");
        await uow.Rooms.AddMemberAsync(membership, ct);

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Oda oluşturuldu: {RoomId} ({Name})", room.Id, room.Name);
        return ToDto(room);
    }

    public async Task<IEnumerable<RoomDto>> GetUserRoomsAsync(Guid userId, CancellationToken ct = default)
    {
        var rooms = await uow.Rooms.GetUserRoomsAsync(userId, ct);
        return rooms.Select(ToDto);
    }

    public async Task JoinRoomAsync(Guid userId, Guid roomId, CancellationToken ct = default)
    {
        if (await uow.Rooms.IsMemberAsync(roomId, userId, ct)) return;

        var membership = RoomMember.Create(roomId, userId);
        await uow.Rooms.AddMemberAsync(membership, ct);
        await uow.SaveChangesAsync(ct);

        await notifier.NotifyRoomAsync(roomId, "room:member_joined", new { userId, roomId });
    }

    public async Task LeaveRoomAsync(Guid userId, Guid roomId, CancellationToken ct = default)
    {
        await notifier.NotifyRoomAsync(roomId, "room:member_left", new { userId, roomId });
        logger.LogInformation("Kullanıcı {UserId} odadan ayrıldı: {RoomId}", userId, roomId);
    }

    private static RoomDto ToDto(Room r) =>
        new(r.Id, r.Name, r.Description, r.Type.ToString(), r.Members.Count, r.CreatedAt);
}