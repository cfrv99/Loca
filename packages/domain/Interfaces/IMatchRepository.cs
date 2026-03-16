using Loca.Domain.Entities;
using Loca.Domain.Enums;

namespace Loca.Domain.Interfaces;

public interface IMatchRepository
{
    Task<MatchRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MatchRequest?> GetPendingAsync(Guid senderId, Guid receiverId, CancellationToken ct = default);
    Task<int> GetDailyRequestCountAsync(Guid senderId, CancellationToken ct = default);
    Task<int> GetDeclineCountAsync(Guid senderId, Guid receiverId, CancellationToken ct = default);
    Task<List<MatchRequest>> GetInboxAsync(Guid userId, MatchRequestStatus? status, int pageSize, string? cursor, CancellationToken ct = default);
    Task AddAsync(MatchRequest request, CancellationToken ct = default);
    Task UpdateAsync(MatchRequest request, CancellationToken ct = default);

    // Conversations
    Task<Conversation?> GetConversationByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Conversation>> GetConversationsAsync(Guid userId, CancellationToken ct = default);
    Task AddConversationAsync(Conversation conversation, CancellationToken ct = default);

    // Blocks
    Task<bool> IsBlockedAsync(Guid userId1, Guid userId2, CancellationToken ct = default);
    Task<Block?> GetBlockAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
    Task AddBlockAsync(Block block, CancellationToken ct = default);
    Task RemoveBlockAsync(Block block, CancellationToken ct = default);

    // Reports
    Task AddReportAsync(Report report, CancellationToken ct = default);
}
