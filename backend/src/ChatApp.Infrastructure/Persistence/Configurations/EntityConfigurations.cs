using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);

        b.HasIndex(u => u.Email).IsUnique();
        b.HasIndex(u => u.Username).IsUnique();

        b.Property(u => u.Email).HasMaxLength(256).IsRequired();
        b.Property(u => u.Username).HasMaxLength(50).IsRequired();
        b.Property(u => u.PasswordHash).IsRequired();
        b.Property(u => u.DisplayName).HasMaxLength(100);
        b.Property(u => u.AvatarUrl).HasMaxLength(512);
    }
}

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasMaxLength(100).IsRequired();
        b.Property(r => r.Description).HasMaxLength(500);

        b.Property(r => r.Type).HasConversion<int>();

        b.HasOne(r => r.CreatedBy)
         .WithMany()
         .HasForeignKey(r => r.CreatedByUserId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoomMemberConfiguration : IEntityTypeConfiguration<RoomMember>
{
    public void Configure(EntityTypeBuilder<RoomMember> b)
    {
        b.HasKey(m => m.Id);

        b.HasIndex(m => new { m.RoomId, m.UserId }).IsUnique();
        b.Property(m => m.Role).HasMaxLength(20).IsRequired();

        b.HasOne(m => m.Room).WithMany(r => r.Members).HasForeignKey(m => m.RoomId);
        b.HasOne(m => m.User).WithMany(u => u.RoomMemberships).HasForeignKey(m => m.UserId);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.HasKey(m => m.Id);
        b.Property(m => m.Content).HasMaxLength(4000).IsRequired();

        b.HasIndex(m => m.RoomId);
        b.HasIndex(m => m.CreatedAt);

        b.HasOne(m => m.Room)
         .WithMany(r => r.Messages)
         .HasForeignKey(m => m.RoomId);

        b.HasOne(m => m.Sender)
         .WithMany(u => u.Messages)
         .HasForeignKey(m => m.SenderId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(m => m.ReplyToMessage)
         .WithMany()
         .HasForeignKey(m => m.ReplyToMessageId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(t => t.Id);
        b.HasIndex(t => t.Token).IsUnique();
        b.Property(t => t.Token).HasMaxLength(256).IsRequired();

        b.HasOne(t => t.User)
         .WithMany(u => u.RefreshTokens)
         .HasForeignKey(t => t.UserId);
    }
}