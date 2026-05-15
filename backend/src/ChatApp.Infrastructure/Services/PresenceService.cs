using ChatApp.Application.Interfaces;
using StackExchange.Redis;

namespace ChatApp.Infrastructure.Services;

public class RedisPresenceService(IConnectionMultiplexer redis) : IPresenceService
{
    private readonly IDatabase _db = redis.GetDatabase();
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(90);

    private static RedisKey UserKey(Guid userId)   => $"presence:user:{userId}";
    private static RedisKey ConnKey(string connId) => $"presence:conn:{connId}";

    public async Task SetOnlineAsync(Guid userId, string connectionId)
    {
        var tx = _db.CreateTransaction();
        _ = tx.SetAddAsync(UserKey(userId), connectionId);
        _ = tx.KeyExpireAsync(UserKey(userId), Ttl);
        _ = tx.StringSetAsync(ConnKey(connectionId), userId.ToString(), Ttl);
        await tx.ExecuteAsync();
    }

    public async Task SetOfflineAsync(string connectionId)
    {
        var userIdStr = await _db.StringGetAsync(ConnKey(connectionId));
        await _db.KeyDeleteAsync(ConnKey(connectionId));

        if (userIdStr.HasValue && Guid.TryParse(userIdStr.ToString(), out var userId))
            await _db.SetRemoveAsync(UserKey(userId), connectionId);
    }

    public async Task<bool> IsOnlineAsync(Guid userId) =>
        await _db.KeyExistsAsync(UserKey(userId));

    public async Task<IEnumerable<Guid>> GetOnlineUsersAsync(IEnumerable<Guid> userIds)
    {
        var online = new List<Guid>();
        foreach (var uid in userIds)
            if (await IsOnlineAsync(uid))
                online.Add(uid);
        return online;
    }
}