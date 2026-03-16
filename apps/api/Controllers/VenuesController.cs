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
[Route("api/v1/venues")]
[Authorize]
public class VenuesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VenuesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get nearby venues with real-time people count
    /// </summary>
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(ApiResponse<CursorPageResponse<VenueCardDto>>), 200)]
    public async Task<IActionResult> GetNearby([FromQuery] GetNearbyVenuesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CursorPageResponse<VenueCardDto>>.Ok(data)),
            error => StatusCode(500, ApiResponse<CursorPageResponse<VenueCardDto>>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get venue detail
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VenueDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<VenueDetailDto>), 404)]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _mediator.Send(new GetVenueDetailQuery(id));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<VenueDetailDto>.Ok(data)),
            error => NotFound(ApiResponse<VenueDetailDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get real-time venue stats from Redis
    /// </summary>
    [HttpGet("{id:guid}/stats")]
    [ProducesResponseType(typeof(ApiResponse<VenueStatsDto>), 200)]
    public async Task<IActionResult> GetStats(Guid id)
    {
        var result = await _mediator.Send(new GetVenueStatsQuery(id));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<VenueStatsDto>.Ok(data)),
            error => NotFound(ApiResponse<VenueStatsDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get people currently at venue (non-anonymous only)
    /// </summary>
    [HttpGet("{id:guid}/people")]
    [ProducesResponseType(typeof(ApiResponse<CursorPageResponse<ActiveUserDto>>), 200)]
    public async Task<IActionResult> GetPeople(Guid id, [FromQuery] string? cursor, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetVenuePeopleQuery(id, cursor, pageSize));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CursorPageResponse<ActiveUserDto>>.Ok(data)),
            error => StatusCode(500, ApiResponse<CursorPageResponse<ActiveUserDto>>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get venue chat message history
    /// </summary>
    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(ApiResponse<CursorPageResponse<ChatMessageDto>>), 200)]
    public async Task<IActionResult> GetMessages(Guid id, [FromQuery] string? cursor, [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetVenueMessagesQuery(id, cursor, pageSize));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CursorPageResponse<ChatMessageDto>>.Ok(data)),
            error => StatusCode(500, ApiResponse<CursorPageResponse<ChatMessageDto>>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get venue feed (memories/posts)
    /// </summary>
    [HttpGet("{id:guid}/feed")]
    [ProducesResponseType(typeof(ApiResponse<CursorPageResponse<PostDto>>), 200)]
    public async Task<IActionResult> GetFeed(Guid id, [FromQuery] string? cursor, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetVenueFeedQuery(id, cursor, pageSize));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CursorPageResponse<PostDto>>.Ok(data)),
            error => StatusCode(500, ApiResponse<CursorPageResponse<PostDto>>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Check in to a venue via QR code
    /// </summary>
    [HttpPost("checkin")]
    [ProducesResponseType(typeof(ApiResponse<CheckInResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CheckInResultDto>), 400)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInCommand cmd)
    {
        cmd = cmd with { UserId = User.GetUserId(), Gender = User.GetGender() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CheckInResultDto>.Ok(data)),
            error => error.Code switch
            {
                "INVALID_QR" => BadRequest(ApiResponse<CheckInResultDto>.Fail(error.Code, error.Message)),
                "OUTSIDE_GEOFENCE" => BadRequest(ApiResponse<CheckInResultDto>.Fail(error.Code, error.Message)),
                "RATE_LIMITED" => StatusCode(429, ApiResponse<CheckInResultDto>.Fail(error.Code, error.Message)),
                _ => StatusCode(500, ApiResponse<CheckInResultDto>.Fail(error.Code, error.Message))
            }
        );
    }

    /// <summary>
    /// Manual checkout
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ApiResponse<CheckOutResultDto>), 200)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
        var cmd = new CheckOutCommand(request.CheckInId) { UserId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CheckOutResultDto>.Ok(data)),
            error => error.Code switch
            {
                "NOT_FOUND" => NotFound(ApiResponse<CheckOutResultDto>.Fail(error.Code, error.Message)),
                "FORBIDDEN" => StatusCode(403, ApiResponse<CheckOutResultDto>.Fail(error.Code, error.Message)),
                _ => BadRequest(ApiResponse<CheckOutResultDto>.Fail(error.Code, error.Message))
            }
        );
    }
}
