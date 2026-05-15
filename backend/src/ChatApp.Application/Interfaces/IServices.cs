using ChatApp.Application.DTOs;

namespace ChatApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthTokensDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthTokensDto> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthTokensDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task                RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
}

public interface IChatService
{
    Task<MessageDto>             SendMessageAsync(Guid senderId, SendMessageRequest request, CancellationToken ct = default);
    Task<IEnumerable<MessageDto>> GetRoomMessagesAsync(Guid roomId, int page, int pageSize, CancellationToken ct = default);
    Task                         DeleteMessageAsync(Guid messageId, Guid requestingUserId, CancellationToken ct = default);
}

public interface IRoomService
{
    Task<RoomDto>             CreateRoomAsync(Guid createdByUserId, CreateRoomRequest request, CancellationToken ct = default);
    Task<IEnumerable<RoomDto>> GetUserRoomsAsync(Guid userId, CancellationToken ct = default);
    Task                       JoinRoomAsync(Guid userId, Guid roomId, CancellationToken ct = default);
    Task                       LeaveRoomAsync(Guid userId, Guid roomId, CancellationToken ct = default);
}

public interface IPresenceService
{
    Task             SetOnlineAsync(Guid userId, string connectionId);
    Task             SetOfflineAsync(string connectionId);
    Task<bool>       IsOnlineAsync(Guid userId);
    Task<IEnumerable<Guid>> GetOnlineUsersAsync(IEnumerable<Guid> userIds);
}

public interface IJwtService
{
    string  GenerateAccessToken(Guid userId, string username, string email);
    string  GenerateRefreshToken();
    Guid?   ValidateAccessToken(string token);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool   Verify(string password, string hash);
}

public interface IChatNotifier
{
    Task NotifyRoomAsync(Guid roomId, string @event, object payload);
    Task NotifyUserAsync(Guid userId, string @event, object payload);
}