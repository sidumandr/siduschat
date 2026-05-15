namespace ChatApp.Domain.Entities;

public class User : BaseEntity
{
    public string  Username     { get; private set; } = null!;
    public string  Email        { get; private set; } = null!;
    public string  PasswordHash { get; private set; } = null!;
    public string? DisplayName  { get; private set; }
    public string? AvatarUrl    { get; private set; }
    public bool    IsActive     { get; private set; } = true;
    public DateTime? LastSeenAt { get; private set; }

    // Navigation properties
    public ICollection<RoomMember>   RoomMemberships { get; private set; } = new List<RoomMember>();
    public ICollection<Message>      Messages        { get; private set; } = new List<Message>();
    public ICollection<RefreshToken> RefreshTokens   { get; private set; } = new List<RefreshToken>();

    private User() { }

    public static User Create(string username, string email, string passwordHash, string? displayName = null)
    {
        return new User
        {
            Username     = username.Trim().ToLowerInvariant(),
            Email        = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            DisplayName  = displayName ?? username,
        };
    }

    public void UpdateLastSeen()
    {
        LastSeenAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void UpdateProfile(string? displayName, string? avatarUrl)
    {
        DisplayName = displayName ?? DisplayName;
        AvatarUrl   = avatarUrl   ?? AvatarUrl;
        MarkUpdated();
    }
}