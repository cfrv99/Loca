using System.Text.Json;
using Loca.Application.Interfaces;
using StackExchange.Redis;

namespace Loca.Infrastructure.Redis;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private IDatabase Db => _redis.GetDatabase();

    public RedisService(IConnectionMultiplexer redis) => _redis = redis;

    // ── Venue counters ──
    public async Task IncrementVenueCountAsync(Guid venueId)
        => await Db.StringIncrementAsync($"venue:{venueId}:count");

    public async Task DecrementVenueCountAsync(Guid venueId)
    {
        var key = $"venue:{venueId}:count";
        var val = await Db.StringDecrementAsync(key);
        if (val < 0) await Db.StringSetAsync(key, 0);
    }

    public async Task<int> GetVenueCountAsync(Guid venueId)
    {
        var val = await Db.StringGetAsync($"venue:{venueId}:count");
        return val.HasValue ? (int)val : 0;
    }

    // ── Active users ──
    public async Task AddActiveUserAsync(Guid venueId, Guid userId)
        => await Db.SetAddAsync($"venue:{venueId}:users", userId.ToString());

    public async Task RemoveActiveUserAsync(Guid venueId, Guid userId)
        => await Db.SetRemoveAsync($"venue:{venueId}:users", userId.ToString());

    public async Task<List<Guid>> GetActiveUsersAsync(Guid venueId)
    {
        var members = await Db.SetMembersAsync($"venue:{venueId}:users");
        return members.Select(m => Guid.Parse(m.ToString())).ToList();
    }

    // ── Chat message cache ──
    public async Task CacheMessageAsync(Guid chatRoomId, string messageJson)
    {
        var key = $"chat:{chatRoomId}:messages";
        await Db.ListRightPushAsync(key, messageJson);
        await Db.ListTrimAsync(key, -100, -1); // Keep last 100
    }

    public async Task<List<string>> GetRecentMessagesAsync(Guid chatRoomId, int count = 100)
    {
        var key = $"chat:{chatRoomId}:messages";
        var messages = await Db.ListRangeAsync(key, -count, -1);
        return messages.Select(m => m.ToString()).ToList();
    }

    // ── Generic cache ──
    public async Task<T?> GetAsync<T>(string key)
    {
        var val = await Db.StringGetAsync(key);
        if (!val.HasValue) return default;
        return JsonSerializer.Deserialize<T>(val.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await Db.StringSetAsync(key, json, expiry);
    }

    public async Task RemoveAsync(string key)
        => await Db.KeyDeleteAsync(key);

    // ── Leaderboard ──
    public async Task IncrementLeaderboardAsync(string leaderboardKey, Guid userId, double score)
        => await Db.SortedSetIncrementAsync(leaderboardKey, userId.ToString(), score);

    public async Task<List<(Guid UserId, double Score)>> GetLeaderboardAsync(string leaderboardKey, int count = 10)
    {
        var entries = await Db.SortedSetRangeByRankWithScoresAsync(leaderboardKey, 0, count - 1, Order.Descending);
        return entries.Select(e => (Guid.Parse(e.Element.ToString()), e.Score)).ToList();
    }
}
