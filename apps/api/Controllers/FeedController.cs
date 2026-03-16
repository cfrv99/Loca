using Loca.API.Extensions;
using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Services.Venue.Commands;
using Loca.Services.Venue.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loca.API.Controllers;

[ApiController]
[Route("api/v1/posts")]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeedController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Create a post (memory) in a venue
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PostDto>), 400)]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var cmd = new CreatePostCommand(request.VenueId, request.Content, request.MediaUrls)
        {
            UserId = User.GetUserId()
        };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<PostDto>.Ok(data)),
            error => BadRequest(ApiResponse<PostDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Toggle like on a post
    /// </summary>
    [HttpPost("{id:guid}/like")]
    [ProducesResponseType(typeof(ApiResponse<ToggleLikeResponse>), 200)]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        var cmd = new ToggleLikeCommand(id) { UserId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<ToggleLikeResponse>.Ok(data)),
            error => NotFound(ApiResponse<ToggleLikeResponse>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get comments on a post
    /// </summary>
    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<CursorPageResponse<CommentDto>>), 200)]
    public async Task<IActionResult> GetComments(Guid id, [FromQuery] string? cursor, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetCommentsQuery(id, cursor, pageSize));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CursorPageResponse<CommentDto>>.Ok(data)),
            error => StatusCode(500, ApiResponse<CursorPageResponse<CommentDto>>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Add a comment to a post
    /// </summary>
    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), 400)]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentRequest request)
    {
        var cmd = new AddCommentCommand(id, request.Content) { UserId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CommentDto>.Ok(data)),
            error => error.Code switch
            {
                "POST_NOT_FOUND" => NotFound(ApiResponse<CommentDto>.Fail(error.Code, error.Message)),
                _ => BadRequest(ApiResponse<CommentDto>.Fail(error.Code, error.Message))
            }
        );
    }
}
