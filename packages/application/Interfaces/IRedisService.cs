namespace Loca.Application.Interfaces;

public interface IRedisService
{
    // Venue counters
    Task IncrementVenueCountAsync(Guid venueId);
    Task DecrementVenueCountAsync(Guid venueId);
    Task<int> GetVenueCountAsync(Guid venueId);

    // Active users
    Task AddActiveUserAsync(Guid venueId, Guid userId);
    Task RemoveActiveUserAsync(Guid venueId, Guid userId);
    Task<List<Guid>> GetActiveUsersAsync(Guid venueId);

    // Chat message cache
    Task CacheMessageAsync(Guid chatRoomId, string messageJson);
    Task<List<string>> GetRecentMessagesAsync(Guid chatRoomId, int count = 100);

    // Generic cache
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);

    // Leaderboard
    Task IncrementLeaderboardAsync(string leaderboardKey, Guid userId, double score);
    Task<List<(Guid UserId, double Score)>> GetLeaderboardAsync(string leaderboardKey, int count = 10);
}
