namespace ChatApp.Domain.Entities;

public class Message : BaseEntity
{
    public Guid    RoomId            { get; private set; }
    public Guid    SenderId          { get; private set; }
    public string  Content           { get; private set; } = null!;
    public bool    IsDeleted         { get; private set; }
    public Guid?   ReplyToMessageId  { get; private set; }

    public Room    Room              { get; private set; } = null!;
    public User    Sender            { get; private set; } = null!;
    public Message? ReplyToMessage   { get; private set; }

    private Message() { }

    public static Message Create(Guid roomId, Guid senderId, string content,
        Guid? replyToMessageId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.", nameof(content));

        return new Message
        {
            RoomId           = roomId,
            SenderId         = senderId,
            Content          = content.Trim(),
            ReplyToMessageId = replyToMessageId,
        };
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        Content   = "[Bu mesaj silindi]";
        MarkUpdated();
    }

    public void Edit(string newContent)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Silinmiş mesaj düzenlenemez.");

        Content = newContent.Trim();
        MarkUpdated();
    }
}