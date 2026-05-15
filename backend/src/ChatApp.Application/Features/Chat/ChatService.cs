using ChatApp.Application.DTOs;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Features.Chat;

public class ChatService(
    IUnitOfWork          uow,
    IChatNotifier        notifier,
    ILogger<ChatService> logger) : IChatService
{
    public async Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageRequest req, CancellationToken ct = default)
    {
        if (!await uow.Rooms.IsMemberAsync(req.RoomId, senderId, ct))
            throw new UnauthorizedAccessException("Bu odanın üyesi değilsiniz.");

        var message = Message.Create(req.RoomId, senderId, req.Content, req.ReplyToMessageId);
        var saved   = await uow.Messages.AddAsync(message, ct);
        await uow.SaveChangesAsync(ct);

        var sender = await uow.Users.GetByIdAsync(senderId, ct);
        var dto    = ToDto(saved, sender!);

        await notifier.NotifyRoomAsync(req.RoomId, "message:new", dto);

        logger.LogInformation("Mesaj gönderildi: {MessageId} → Oda {RoomId}", saved.Id, req.RoomId);
        return dto;
    }

    public async Task<IEnumerable<MessageDto>> GetRoomMessagesAsync(
        Guid roomId, int page, int pageSize, CancellationToken ct = default)
    {
        var messages   = await uow.Messages.GetRoomMessagesAsync(roomId, page, pageSize, ct);
        var senderIds  = messages.Select(m => m.SenderId).Distinct();

        var senders = new Dictionary<Guid, UserDto>();
        foreach (var sid in senderIds)
        {
            var u = await uow.Users.GetByIdAsync(sid, ct);
            if (u is not null)
                senders[sid] = new UserDto(u.Id, u.Username, u.DisplayName, u.AvatarUrl, false, u.LastSeenAt);
        }

        return messages.Select(m => ToDto(m, senders.GetValueOrDefault(m.SenderId)!));
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid requestingUserId, CancellationToken ct = default)
    {
        var message = await uow.Messages.GetByIdAsync(messageId, ct)
            ?? throw new KeyNotFoundException("Mesaj bulunamadı.");

        if (message.SenderId != requestingUserId)
            throw new UnauthorizedAccessException("Başkasının mesajını silemezsiniz.");

        message.SoftDelete();
        await uow.Messages.UpdateAsync(message, ct);
        await uow.SaveChangesAsync(ct);

        await notifier.NotifyRoomAsync(message.RoomId, "message:deleted", new { messageId, roomId = message.RoomId });
    }

    private static MessageDto ToDto(Message m, UserDto sender) =>
        new(m.Id, m.RoomId, sender, m.Content, m.IsDeleted, m.ReplyToMessageId, m.CreatedAt, m.UpdatedAt);

    private static MessageDto ToDto(Message m, User sender) =>
        ToDto(m, new UserDto(sender.Id, sender.Username, sender.DisplayName, sender.AvatarUrl, false, sender.LastSeenAt));
}