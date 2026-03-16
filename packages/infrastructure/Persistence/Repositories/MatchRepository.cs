using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loca.Infrastructure.Persistence.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly LocaDbContext _db;

    public MatchRepository(LocaDbContext db) => _db = db;

    public async Task<MatchRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.MatchRequests
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<MatchRequest?> GetPendingAsync(Guid senderId, Guid receiverId, CancellationToken ct = default)
        => await _db.MatchRequests.FirstOrDefaultAsync(m =>
            m.SenderId == senderId && m.ReceiverId == receiverId && m.Status == MatchRequestStatus.Pending, ct);

    public async Task<int> GetDailyRequestCountAsync(Guid senderId, CancellationToken ct = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        return await _db.MatchRequests.CountAsync(m =>
            m.SenderId == senderId && m.CreatedAt >= todayStart, ct);
    }

    public async Task<int> GetDeclineCountAsync(Guid senderId, Guid receiverId, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-48);
        return await _db.MatchRequests.CountAsync(m =>
            m.SenderId == senderId && m.ReceiverId == receiverId &&
            m.Status == MatchRequestStatus.Declined && m.RespondedAt >= cutoff, ct);
    }

    public async Task<List<MatchRequest>> GetInboxAsync(Guid userId, MatchRequestStatus? status, int pageSize, string? cursor, CancellationToken ct = default)
    {
        var query = _db.MatchRequests
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ReceiverId == userId || m.SenderId == userId);

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        query = query.OrderByDescending(m => m.CreatedAt);

        if (!string.IsNullOrEmpty(cursor) && Guid.TryParse(cursor, out var cursorId))
        {
            var cursorItem = await _db.MatchRequests.FirstOrDefaultAsync(m => m.Id == cursorId, ct);
            if (cursorItem is not null)
                query = query.Where(m => m.CreatedAt < cursorItem.CreatedAt);
        }

        return await query.Take(pageSize).ToListAsync(ct);
    }

    public async Task AddAsync(MatchRequest request, CancellationToken ct = default)
    {
        _db.MatchRequests.Add(request);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MatchRequest request, CancellationToken ct = default)
    {
        request.UpdatedAt = DateTime.UtcNow;
        _db.MatchRequests.Update(request);
        await _db.SaveChangesAsync(ct);
    }

    // Conversations
    public async Task<Conversation?> GetConversationByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Conversations.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<Conversation>> GetConversationsAsync(Guid userId, CancellationToken ct = default)
        => await _db.Conversations
            .Where(c => c.Participant1Id == userId || c.Participant2Id == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(ct);

    public async Task AddConversationAsync(Conversation conversation, CancellationToken ct = default)
    {
        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync(ct);
    }

    // Blocks
    public async Task<bool> IsBlockedAsync(Guid userId1, Guid userId2, CancellationToken ct = default)
        => await _db.Blocks.AnyAsync(b =>
            (b.BlockerId == userId1 && b.BlockedId == userId2) ||
            (b.BlockerId == userId2 && b.BlockedId == userId1), ct);

    public async Task<Block?> GetBlockAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default)
        => await _db.Blocks.FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId, ct);

    public async Task AddBlockAsync(Block block, CancellationToken ct = default)
    {
        _db.Blocks.Add(block);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveBlockAsync(Block block, CancellationToken ct = default)
    {
        _db.Blocks.Remove(block);
        await _db.SaveChangesAsync(ct);
    }

    // Reports
    public async Task AddReportAsync(Report report, CancellationToken ct = default)
    {
        _db.Reports.Add(report);
        await _db.SaveChangesAsync(ct);
    }
}
