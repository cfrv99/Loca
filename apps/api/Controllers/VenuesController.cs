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
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
        // TODO: Implement checkout
        return Ok(ApiResponse<object>.Ok(new { success = true }));
    }
}
