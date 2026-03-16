using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Infrastructure.Persistence;
using Loca.Services.Venue.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class GetVenueMessagesHandler : IRequestHandler<GetVenueMessagesQuery, Result<CursorPageResponse<ChatMessageDto>>>
{
    private readonly LocaDbContext _db;
    private readonly IRedisService _redis;
    private readonly ILogger<GetVenueMessagesHandler> _logger;

    public GetVenueMessagesHandler(LocaDbContext db, IRedisService redis, ILogger<GetVenueMessagesHandler> logger)
    {
        _db = db;
        _redis = redis;
        _logger = logger;
    }

    public async Task<Result<CursorPageResponse<ChatMessageDto>>> Handle(GetVenueMessagesQuery query, CancellationToken ct)
    {
        var roomId = $"venue_{query.VenueId}";

        // If no cursor, try Redis cache first for latest messages
        if (string.IsNullOrEmpty(query.Cursor))
        {
            var cached = await _redis.GetCachedMessagesAsync(roomId, query.PageSize);
            if (cached.Count > 0)
            {
                return Result<CursorPageResponse<ChatMessageDto>>.Success(new CursorPageResponse<ChatMessageDto>
                {
                    Items = cached,
                    NextCursor = cached.Count >= query.PageSize ? cached.First().Id.ToString() : null,
                    HasMore = cached.Count >= query.PageSize
                });
            }
        }

        // Fall back to DB
        var messagesQuery = _db.Messages
            .Where(m => m.RoomId == roomId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt);

        if (!string.IsNullOrEmpty(query.Cursor) && Guid.TryParse(query.Cursor, out var cursorId))
        {
            var cursorItem = await _db.Messages.FirstOrDefaultAsync(m => m.Id == cursorId, ct);
            if (cursorItem is not null)
                messagesQuery = (IOrderedQueryable<Domain.Entities.ChatMessage>)messagesQuery
                    .Where(m => m.CreatedAt < cursorItem.CreatedAt);
        }

        var messages = await messagesQuery
            .Take(query.PageSize + 1)
            .ToListAsync(ct);

        var hasMore = messages.Count > query.PageSize;
        var items = messages.Take(query.PageSize).ToList();

        // Map to DTOs with sender info
        var senderIds = items.Select(m => m.SenderId).Distinct().ToList();
        var senders = await _db.Users
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var dtos = items.Select(m =>
        {
            senders.TryGetValue(m.SenderId, out var sender);
            return new ChatMessageDto(
                Id: m.Id,
                SenderId: m.SenderId,
                SenderName: sender?.DisplayName ?? "Guest",
                SenderAvatar: sender?.AvatarUrl,
                Type: m.MessageType.ToString().ToLower(),
                Content: m.Content,
                MediaUrl: m.MediaUrl,
                ReplyTo: null,
                Metadata: null,
                CreatedAt: m.CreatedAt
            );
        }).ToList();

        return Result<CursorPageResponse<ChatMessageDto>>.Success(new CursorPageResponse<ChatMessageDto>
        {
            Items = dtos,
            NextCursor = hasMore && items.Count > 0 ? items.Last().Id.ToString() : null,
            HasMore = hasMore
        });
    }
}
