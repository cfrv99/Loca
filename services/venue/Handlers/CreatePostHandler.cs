using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, Result<PostDto>>
{
    private readonly IPostRepository _posts;
    private readonly IUserRepository _users;
    private readonly IVenueRepository _venues;
    private readonly ILogger<CreatePostHandler> _logger;

    public CreatePostHandler(IPostRepository posts, IUserRepository users, IVenueRepository venues, ILogger<CreatePostHandler> logger)
    {
        _posts = posts;
        _users = users;
        _venues = venues;
        _logger = logger;
    }

    public async Task<Result<PostDto>> Handle(CreatePostCommand cmd, CancellationToken ct)
    {
        var venue = await _venues.GetByIdAsync(cmd.VenueId, ct);
        if (venue is null)
            return Result<PostDto>.Failure("VENUE_NOT_FOUND", "Məkan tapılmadı");

        var user = await _users.GetByIdAsync(cmd.UserId, ct);
        if (user is null)
            return Result<PostDto>.Failure("USER_NOT_FOUND", "İstifadəçi tapılmadı");

        var post = new Post
        {
            UserId = cmd.UserId,
            VenueId = cmd.VenueId,
            Content = cmd.Content,
            MediaUrls = cmd.MediaUrls ?? new List<string>(),
            IsMemory = true
        };

        await _posts.AddAsync(post, ct);
        _logger.LogInformation("User {UserId} created post {PostId} in venue {VenueId}", cmd.UserId, post.Id, cmd.VenueId);

        return Result<PostDto>.Success(new PostDto(
            Id: post.Id,
            UserId: post.UserId,
            UserName: user.DisplayName,
            UserAvatar: user.AvatarUrl,
            Content: post.Content,
            MediaUrls: post.MediaUrls,
            LikeCount: 0,
            CommentCount: 0,
            IsLikedByMe: false,
            CreatedAt: post.CreatedAt
        ));
    }
}
