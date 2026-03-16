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
    /// Get venue details
    /// </summary>
    [HttpGet("{venueId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VenueDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<VenueDetailDto>), 404)]
    public async Task<IActionResult> GetDetail(Guid venueId)
    {
        var result = await _mediator.Send(new GetVenueDetailQuery(venueId));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<VenueDetailDto>.Ok(data)),
            error => NotFound(ApiResponse<VenueDetailDto>.Fail(error.Code, error.Message))
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
        cmd = cmd with { UserId = User.GetUserId() };
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
    /// Check out from a venue
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutCommand cmd)
    {
        cmd = cmd with { UserId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<bool>.Ok(data)),
            error => BadRequest(ApiResponse<bool>.Fail(error.Code, error.Message))
        );
    }
}
