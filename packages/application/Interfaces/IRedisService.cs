using Loca.Application.DTOs;

namespace Loca.Application.Interfaces;

public interface IRedisService
{
    // Venue counters
    Task IncrementVenueCountAsync(Guid venueId, string gender = "unknown");
    Task DecrementVenueCountAsync(Guid venueId, string gender = "unknown");
    Task<VenueStatsDto> GetVenueStatsAsync(Guid venueId);

    // Active users
    Task AddActiveUserAsync(Guid venueId, Guid userId);
    Task RemoveActiveUserAsync(Guid venueId, Guid userId);
    Task<HashSet<Guid>> GetActiveUserIdsAsync(Guid venueId);

    // User state
    Task SetUserOnlineAsync(Guid userId, bool online);
    Task<bool> IsUserOnlineAsync(Guid userId);
    Task<DateTime?> GetLastSeenAsync(Guid userId);

    // User venues
    Task AddUserToVenueAsync(Guid userId, Guid venueId);
    Task RemoveUserFromVenueAsync(Guid userId, Guid venueId);
    Task<List<Guid>> GetUserVenuesAsync(Guid userId);

    // Chat cache
    Task CacheMessageAsync(string roomId, ChatMessageDto message);
    Task<List<ChatMessageDto>> GetCachedMessagesAsync(string roomId, int count = 100);

    // Conversation groups (private chat)
    Task AddUserToConversationAsync(Guid userId, Guid conversationId);
    Task RemoveUserFromConversationAsync(Guid userId, Guid conversationId);
    Task<List<Guid>> GetUserConversationsAsync(Guid userId);

    // Generic
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<bool> DeleteAsync(string key);
}
