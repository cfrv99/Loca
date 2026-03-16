using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Infrastructure.Persistence;
using Loca.Services.Venue.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Loca.Services.Venue.Handlers;

public class GetCommentsHandler : IRequestHandler<GetCommentsQuery, Result<CursorPageResponse<CommentDto>>>
{
    private readonly IPostRepository _posts;
    private readonly LocaDbContext _db;

    public GetCommentsHandler(IPostRepository posts, LocaDbContext db)
    {
        _posts = posts;
        _db = db;
    }

    public async Task<Result<CursorPageResponse<CommentDto>>> Handle(GetCommentsQuery query, CancellationToken ct)
    {
        var comments = await _posts.GetCommentsAsync(query.PostId, query.PageSize + 1, query.Cursor, ct);
        var hasMore = comments.Count > query.PageSize;
        var items = comments.Take(query.PageSize).ToList();

        // Get user info
        var userIds = items.Select(c => c.UserId).Distinct().ToList();
        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var dtos = items.Select(c =>
        {
            users.TryGetValue(c.UserId, out var user);
            return new CommentDto(
                Id: c.Id,
                UserId: c.UserId,
                UserName: user?.DisplayName ?? "Guest",
                UserAvatar: user?.AvatarUrl,
                Content: c.Content,
                CreatedAt: c.CreatedAt
            );
        }).ToList();

        return Result<CursorPageResponse<CommentDto>>.Success(new CursorPageResponse<CommentDto>
        {
            Items = dtos,
            NextCursor = hasMore && items.Count > 0 ? items.Last().Id.ToString() : null,
            HasMore = hasMore
        });
    }
}
