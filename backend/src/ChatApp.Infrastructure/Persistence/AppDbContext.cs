using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User>            Users            => Set<User>();
    public DbSet<Room>            Rooms            => Set<Room>();
    public DbSet<RoomMember>      RoomMembers      => Set<RoomMember>();
    public DbSet<Message>         Messages         => Set<Message>();
    public DbSet<RefreshToken>    RefreshTokens    => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}