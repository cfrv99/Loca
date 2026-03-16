using System.Text.Json;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Loca.Infrastructure.Redis;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisService> _logger;
    private IDatabase Db => _redis.GetDatabase();

    public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    // Venue counters
    public async Task IncrementVenueCountAsync(Guid venueId, string gender = "unknown")
    {
        var key = $"venue:{venueId}:stats";
        await Db.HashIncrementAsync(key, "total", 1);
        await Db.HashIncrementAsync(key, gender, 1);
    }

    public async Task DecrementVenueCountAsync(Guid venueId, string gender = "unknown")
    {
        var key = $"venue:{venueId}:stats";
        await Db.HashDecrementAsync(key, "total", 1);
        await Db.HashDecrementAsync(key, gender, 1);
    }

    public async Task<VenueStatsDto> GetVenueStatsAsync(Guid venueId)
    {
        var key = $"venue:{venueId}:stats";
        var hash = await Db.HashGetAllAsync(key);
        var dict = hash.ToDictionary(h => h.Name.ToString(), h => (int)h.Value);
        return new VenueStatsDto(
            Total: dict.GetValueOrDefault("total", 0),
            Male: dict.GetValueOrDefault("male", 0),
            Female: dict.GetValueOrDefault("female", 0)
        );
    }

    // Active users
    public async Task AddActiveUserAsync(Guid venueId, Guid userId)
    {
        var key = $"venue:{venueId}:active_users";
        await Db.SetAddAsync(key, userId.ToString());
    }

    public async Task RemoveActiveUserAsync(Guid venueId, Guid userId)
    {
        var key = $"venue:{venueId}:active_users";
        await Db.SetRemoveAsync(key, userId.ToString());
    }

    public async Task<HashSet<Guid>> GetActiveUserIdsAsync(Guid venueId)
    {
        var key = $"venue:{venueId}:active_users";
        var members = await Db.SetMembersAsync(key);
        return members.Select(m => Guid.Parse(m.ToString())).ToHashSet();
    }

    // User state
    public async Task SetUserOnlineAsync(Guid userId, bool online)
    {
        var key = $"user:{userId}:online";
        if (online)
        {
            await Db.StringSetAsync(key, "1", TimeSpan.FromMinutes(5));
        }
        else
        {
            await Db.KeyDeleteAsync(key);
            await Db.StringSetAsync($"user:{userId}:last_seen", DateTime.UtcNow.ToString("O"), TimeSpan.FromDays(7));
        }
    }

    public async Task<bool> IsUserOnlineAsync(Guid userId)
    {
        var key = $"user:{userId}:online";
        return await Db.KeyExistsAsync(key);
    }

    public async Task<DateTime?> GetLastSeenAsync(Guid userId)
    {
        var val = await Db.StringGetAsync($"user:{userId}:last_seen");
        return val.HasValue ? DateTime.Parse(val.ToString()) : null;
    }

    // User venues
    public async Task AddUserToVenueAsync(Guid userId, Guid venueId)
    {
        await Db.SetAddAsync($"user:{userId}:venues", venueId.ToString());
    }

    public async Task RemoveUserFromVenueAsync(Guid userId, Guid venueId)
    {
        await Db.SetRemoveAsync($"user:{userId}:venues", venueId.ToString());
    }

    public async Task<List<Guid>> GetUserVenuesAsync(Guid userId)
    {
        var members = await Db.SetMembersAsync($"user:{userId}:venues");
        return members.Select(m => Guid.Parse(m.ToString())).ToList();
    }

    // Chat cache
    public async Task CacheMessageAsync(string roomId, ChatMessageDto message)
    {
        var key = $"chat:{roomId}:messages";
        var json = JsonSerializer.Serialize(message);
        await Db.ListRightPushAsync(key, json);
        await Db.ListTrimAsync(key, -100, -1); // Keep last 100
    }

    public async Task<List<ChatMessageDto>> GetCachedMessagesAsync(string roomId, int count = 100)
    {
        var key = $"chat:{roomId}:messages";
        var values = await Db.ListRangeAsync(key, -count, -1);
        return values
            .Select(v => JsonSerializer.Deserialize<ChatMessageDto>(v.ToString())!)
            .ToList();
    }

    // Conversation groups (private chat)
    public async Task AddUserToConversationAsync(Guid userId, Guid conversationId)
    {
        await Db.SetAddAsync($"user:{userId}:conversations", conversationId.ToString());
    }

    public async Task RemoveUserFromConversationAsync(Guid userId, Guid conversationId)
    {
        await Db.SetRemoveAsync($"user:{userId}:conversations", conversationId.ToString());
    }

    public async Task<List<Guid>> GetUserConversationsAsync(Guid userId)
    {
        var members = await Db.SetMembersAsync($"user:{userId}:conversations");
        return members.Select(m => Guid.Parse(m.ToString())).ToList();
    }

    // Generic
    public async Task<string?> GetAsync(string key)
    {
        var val = await Db.StringGetAsync(key);
        return val.HasValue ? val.ToString() : null;
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        await Db.StringSetAsync(key, value, expiry);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await Db.KeyDeleteAsync(key);
    }
}
