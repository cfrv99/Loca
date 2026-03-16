using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class ToggleLikeHandler : IRequestHandler<ToggleLikeCommand, Result<ToggleLikeResponse>>
{
    private readonly IPostRepository _posts;
    private readonly ILogger<ToggleLikeHandler> _logger;

    public ToggleLikeHandler(IPostRepository posts, ILogger<ToggleLikeHandler> logger)
    {
        _posts = posts;
        _logger = logger;
    }

    public async Task<Result<ToggleLikeResponse>> Handle(ToggleLikeCommand cmd, CancellationToken ct)
    {
        var post = await _posts.GetByIdAsync(cmd.PostId, ct);
        if (post is null)
            return Result<ToggleLikeResponse>.Failure("POST_NOT_FOUND", "Post tapılmadı");

        var existingLike = await _posts.GetLikeAsync(cmd.PostId, cmd.UserId, ct);
        bool liked;

        if (existingLike is not null)
        {
            await _posts.RemoveLikeAsync(existingLike, ct);
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
            liked = false;
        }
        else
        {
            await _posts.AddLikeAsync(new Like { PostId = cmd.PostId, UserId = cmd.UserId }, ct);
            post.LikeCount++;
            liked = true;
        }

        await _posts.UpdateAsync(post, ct);
        _logger.LogInformation("User {UserId} {Action} post {PostId}", cmd.UserId, liked ? "liked" : "unliked", cmd.PostId);

        return Result<ToggleLikeResponse>.Success(new ToggleLikeResponse(liked, post.LikeCount));
    }
}
