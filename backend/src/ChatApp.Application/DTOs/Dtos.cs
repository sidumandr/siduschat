namespace ChatApp.Application.DTOs;

// response

public record UserDto(
    Guid    Id,
    string  Username,
    string? DisplayName,
    string? AvatarUrl,
    bool    IsOnline,
    DateTime? LastSeenAt
);

public record RoomDto(
    Guid     Id,
    string   Name,
    string?  Description,
    string   Type,
    int      MemberCount,
    DateTime CreatedAt
);

public record MessageDto(
    Guid      Id,
    Guid      RoomId,
    UserDto   Sender,
    string    Content,
    bool      IsDeleted,
    Guid?     ReplyToMessageId,
    DateTime  CreatedAt,
    DateTime? UpdatedAt
);

public record AuthTokensDto(
    string  AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

// request

public record RegisterRequest(
    string  Username,
    string  Email,
    string  Password,
    string? DisplayName
);

public record LoginRequest(
    string Email,
    string Password
);

public record SendMessageRequest(
    Guid    RoomId,
    string  Content,
    Guid?   ReplyToMessageId = null
);

public record CreateRoomRequest(
    string  Name,
    string? Description,
    string  Type = "Public"
);