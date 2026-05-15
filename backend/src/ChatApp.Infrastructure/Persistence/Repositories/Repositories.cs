using ChatApp.Domain.Entities;
using ChatApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant(), ct);

    public Task<bool> ExistsAsync(string email, string username, CancellationToken ct = default) =>
        db.Users.AnyAsync(
            u => u.Email == email.ToLowerInvariant() || u.Username == username.ToLowerInvariant(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        return Task.CompletedTask;
    }
}

public class RoomRepository(AppDbContext db) : IRoomRepository
{
    public Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Rooms
          .Include(r => r.Members)
          .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<Room>> GetUserRoomsAsync(Guid userId, CancellationToken ct = default) =>
        await db.Rooms
                .Where(r => r.Members.Any(m => m.UserId == userId) && !r.IsArchived)
                .Include(r => r.Members)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);

    public Task<Room?> GetDirectRoomAsync(Guid userAId, Guid userBId, CancellationToken ct = default)
    {
        var ids  = new[] { userAId, userBId }.OrderBy(x => x).ToArray();
        var name = $"dm-{ids[0]}-{ids[1]}";
        return db.Rooms.FirstOrDefaultAsync(r => r.Name == name, ct);
    }

    public async Task AddAsync(Room room, CancellationToken ct = default) =>
        await db.Rooms.AddAsync(room, ct);

    public async Task AddMemberAsync(RoomMember member, CancellationToken ct = default) =>
        await db.RoomMembers.AddAsync(member, ct);

    public Task<bool> IsMemberAsync(Guid roomId, Guid userId, CancellationToken ct = default) =>
        db.RoomMembers.AnyAsync(m => m.RoomId == roomId && m.UserId == userId, ct);
}

public class MessageRepository(AppDbContext db) : IMessageRepository
{
    public Task<Message?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Messages
          .Include(m => m.Sender)
          .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IEnumerable<Message>> GetRoomMessagesAsync(
        Guid roomId, int page, int pageSize, CancellationToken ct = default) =>
        await db.Messages
                .Where(m => m.RoomId == roomId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

    public async Task<Message> AddAsync(Message message, CancellationToken ct = default)
    {
        var entry = await db.Messages.AddAsync(message, ct);
        return entry.Entity;
    }

    public Task UpdateAsync(Message message, CancellationToken ct = default)
    {
        db.Messages.Update(message);
        return Task.CompletedTask;
    }
}

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken ct = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(
            t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await db.RefreshTokens.AddAsync(token, ct);

    public async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        var tokens = await db.RefreshTokens
                             .Where(t => t.UserId == userId && !t.IsRevoked)
                             .ToListAsync(ct);
        foreach (var t in tokens) t.Revoke(reason);
    }
}

public class UnitOfWork(
    AppDbContext            db,
    IUserRepository         users,
    IRoomRepository         rooms,
    IMessageRepository      messages,
    IRefreshTokenRepository refreshTokens) : IUnitOfWork
{
    public IUserRepository         Users         => users;
    public IRoomRepository         Rooms         => rooms;
    public IMessageRepository      Messages      => messages;
    public IRefreshTokenRepository RefreshTokens => refreshTokens;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}