using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class AddCommentHandler : IRequestHandler<AddCommentCommand, Result<CommentDto>>
{
    private readonly IPostRepository _posts;
    private readonly IUserRepository _users;
    private readonly ILogger<AddCommentHandler> _logger;

    public AddCommentHandler(IPostRepository posts, IUserRepository users, ILogger<AddCommentHandler> logger)
    {
        _posts = posts;
        _users = users;
        _logger = logger;
    }

    public async Task<Result<CommentDto>> Handle(AddCommentCommand cmd, CancellationToken ct)
    {
        var post = await _posts.GetByIdAsync(cmd.PostId, ct);
        if (post is null)
            return Result<CommentDto>.Failure("POST_NOT_FOUND", "Post tapılmadı");

        var user = await _users.GetByIdAsync(cmd.UserId, ct);
        if (user is null)
            return Result<CommentDto>.Failure("USER_NOT_FOUND", "İstifadəçi tapılmadı");

        var comment = new Comment
        {
            PostId = cmd.PostId,
            UserId = cmd.UserId,
            Content = cmd.Content
        };

        await _posts.AddCommentAsync(comment, ct);
        post.CommentCount++;
        await _posts.UpdateAsync(post, ct);

        _logger.LogInformation("User {UserId} commented on post {PostId}", cmd.UserId, cmd.PostId);

        return Result<CommentDto>.Success(new CommentDto(
            Id: comment.Id,
            UserId: comment.UserId,
            UserName: user.DisplayName,
            UserAvatar: user.AvatarUrl,
            Content: comment.Content,
            CreatedAt: comment.CreatedAt
        ));
    }
}
