namespace ChatApp.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid     UserId        { get; private set; }
    public string   Token         { get; private set; } = null!;
    public DateTime ExpiresAt     { get; private set; }
    public bool     IsRevoked     { get; private set; }
    public string?  RevokedReason { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
        => new() { UserId = userId, Token = token, ExpiresAt = expiresAt };

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive  => !IsRevoked && !IsExpired;

    public void Revoke(string reason = "manual")
    {
        IsRevoked     = true;
        RevokedReason = reason;
        MarkUpdated();
    }
}