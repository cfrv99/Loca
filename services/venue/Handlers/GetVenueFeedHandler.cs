using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class GetVenueFeedHandler : IRequestHandler<GetVenueFeedQuery, Result<CursorPageResponse<PostDto>>>
{
    private readonly IPostRepository _posts;
    private readonly ILogger<GetVenueFeedHandler> _logger;

    public GetVenueFeedHandler(IPostRepository posts, ILogger<GetVenueFeedHandler> logger)
    {
        _posts = posts;
        _logger = logger;
    }

    public async Task<Result<CursorPageResponse<PostDto>>> Handle(GetVenueFeedQuery query, CancellationToken ct)
    {
        var posts = await _posts.GetVenueFeedAsync(query.VenueId, query.PageSize + 1, query.Cursor, ct);

        var hasMore = posts.Count > query.PageSize;
        var items = posts.Take(query.PageSize).ToList();

        var dtos = items.Select(p => new PostDto(
            Id: p.Id,
            UserId: p.UserId,
            UserName: p.User?.DisplayName ?? "Guest",
            UserAvatar: p.User?.AvatarUrl,
            Content: p.Content,
            MediaUrls: p.MediaUrls,
            LikeCount: p.LikeCount,
            CommentCount: p.CommentCount,
            IsLikedByMe: false, // Would need UserId from caller to check
            CreatedAt: p.CreatedAt
        )).ToList();

        return Result<CursorPageResponse<PostDto>>.Success(new CursorPageResponse<PostDto>
        {
            Items = dtos,
            NextCursor = hasMore && items.Count > 0 ? items.Last().Id.ToString() : null,
            HasMore = hasMore
        });
    }
}
