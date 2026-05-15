namespace ChatApp.Domain.Entities;

public class RoomMember : BaseEntity
{
    public Guid    RoomId      { get; private set; }
    public Guid    UserId      { get; private set; }
    public string  Role        { get; private set; } = "member";
    public DateTime? LastReadAt { get; private set; }

    public Room Room { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private RoomMember() { }

    public static RoomMember Create(Guid roomId, Guid userId, string role = "member")
        => new() { RoomId = roomId, UserId = userId, Role = role };

    public void MarkRead()
    {
        LastReadAt = DateTime.UtcNow;
        MarkUpdated();
    }
}