using ChatApp.Domain.Entities;

namespace ChatApp.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?>  GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?>  GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?>  GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool>   ExistsAsync(string email, string username, CancellationToken ct = default);
    Task         AddAsync(User user, CancellationToken ct = default);
    Task         UpdateAsync(User user, CancellationToken ct = default);
}

public interface IRoomRepository
{
    Task<Room?>             GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Room>> GetUserRoomsAsync(Guid userId, CancellationToken ct = default);
    Task<Room?>             GetDirectRoomAsync(Guid userAId, Guid userBId, CancellationToken ct = default);
    Task                    AddAsync(Room room, CancellationToken ct = default);
    Task                    AddMemberAsync(RoomMember member, CancellationToken ct = default);
    Task<bool>              IsMemberAsync(Guid roomId, Guid userId, CancellationToken ct = default);
}

public interface IMessageRepository
{
    Task<Message?>             GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Message>> GetRoomMessagesAsync(Guid roomId, int page, int pageSize, CancellationToken ct = default);
    Task<Message>              AddAsync(Message message, CancellationToken ct = default);
    Task                       UpdateAsync(Message message, CancellationToken ct = default);
}

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken ct = default);
    Task                AddAsync(RefreshToken token, CancellationToken ct = default);
    Task                RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    IUserRepository         Users         { get; }
    IRoomRepository         Rooms         { get; }
    IMessageRepository      Messages      { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}