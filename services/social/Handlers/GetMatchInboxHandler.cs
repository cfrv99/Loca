using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Social.Queries;
using MediatR;

namespace Loca.Services.Social.Handlers;

public class GetMatchInboxHandler : IRequestHandler<GetMatchInboxQuery, Result<CursorPageResponse<MatchRequestDto>>>
{
    private readonly IMatchRepository _matches;

    public GetMatchInboxHandler(IMatchRepository matches) => _matches = matches;

    public async Task<Result<CursorPageResponse<MatchRequestDto>>> Handle(GetMatchInboxQuery query, CancellationToken ct)
    {
        MatchRequestStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<MatchRequestStatus>(query.Status, true, out var parsed))
            statusFilter = parsed;

        var requests = await _matches.GetInboxAsync(query.UserId, statusFilter, query.PageSize + 1, query.Cursor, ct);

        var hasMore = requests.Count > query.PageSize;
        var items = requests.Take(query.PageSize).ToList();

        var dtos = items.Select(r => new MatchRequestDto(
            Id: r.Id,
            SenderId: r.SenderId,
            SenderName: r.Sender?.DisplayName ?? "Guest",
            SenderAvatar: r.Sender?.AvatarUrl,
            IntroMessage: r.IntroMessage,
            VenueId: r.VenueId,
            Status: r.Status.ToString(),
            CreatedAt: r.CreatedAt
        )).ToList();

        return Result<CursorPageResponse<MatchRequestDto>>.Success(new CursorPageResponse<MatchRequestDto>
        {
            Items = dtos,
            NextCursor = hasMore && items.Count > 0 ? items.Last().Id.ToString() : null,
            HasMore = hasMore
        });
    }
}
