using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

public class Room : BaseEntity
{
    public string  Name            { get; private set; } = null!;
    public string? Description     { get; private set; }
    public RoomType Type           { get; private set; }
    public Guid    CreatedByUserId { get; private set; }
    public bool    IsArchived      { get; private set; }

    // Navigation properties 
    public User                  CreatedBy { get; private set; } = null!;
    public ICollection<RoomMember> Members { get; private set; } = new List<RoomMember>();
    public ICollection<Message>   Messages { get; private set; } = new List<Message>();

    private Room() { }

    public static Room CreatePublic(string name, Guid createdByUserId, string? description = null)
    {
        return new Room
        {
            Name            = name.Trim(),
            Description     = description,
            Type            = RoomType.Public,
            CreatedByUserId = createdByUserId,
        };
    }

    public static Room CreatePrivate(string name, Guid createdByUserId)
    {
        return new Room
        {
            Name            = name.Trim(),
            Type            = RoomType.Private,
            CreatedByUserId = createdByUserId,
        };
    }

    public static Room CreateDirect(Guid userAId, Guid userBId)
    {
        var ids  = new[] { userAId, userBId }.OrderBy(x => x).ToArray();
        return new Room
        {
            Name            = $"dm-{ids[0]}-{ids[1]}",
            Type            = RoomType.DirectMessage,
            CreatedByUserId = userAId,
        };
    }

    public void Archive()
    {
        IsArchived = true;
        MarkUpdated();
    }
}