using Loca.API.Extensions;
using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Services.Social.Commands;
using Loca.Services.Social.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loca.API.Controllers;

[ApiController]
[Route("api/v1/matches")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Send a match request to another user (same venue required)
    /// </summary>
    [HttpPost("request")]
    [ProducesResponseType(typeof(ApiResponse<MatchRequestDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<MatchRequestDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<MatchRequestDto>), 429)]
    public async Task<IActionResult> SendRequest([FromBody] SendMatchRequest request)
    {
        var cmd = new SendMatchRequestCommand(request.ReceiverId, request.IntroMessage)
        {
            UserId = User.GetUserId()
        };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<MatchRequestDto>.Ok(data)),
            error => error.Code switch
            {
                "DAILY_LIMIT_REACHED" => StatusCode(429, ApiResponse<MatchRequestDto>.Fail(error.Code, error.Message)),
                "USER_BLOCKED" => StatusCode(403, ApiResponse<MatchRequestDto>.Fail(error.Code, error.Message)),
                _ => BadRequest(ApiResponse<MatchRequestDto>.Fail(error.Code, error.Message))
            }
        );
    }

    /// <summary>
    /// Accept or decline a match request
    /// </summary>
    [HttpPut("{id:guid}/respond")]
    [ProducesResponseType(typeof(ApiResponse<RespondToMatchResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RespondToMatchResult>), 400)]
    public async Task<IActionResult> Respond(Guid id, [FromBody] RespondToMatchRequest request)
    {
        var cmd = new RespondToMatchCommand(id, request.Action)
        {
            UserId = User.GetUserId()
        };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<RespondToMatchResult>.Ok(data)),
            error => error.Code switch
            {
                "NOT_FOUND" => NotFound(ApiResponse<RespondToMatchResult>.Fail(error.Code, error.Message)),
                "FORBIDDEN" => StatusCode(403, ApiResponse<RespondToMatchResult>.Fail(error.Code, error.Message)),
                _ => BadRequest(ApiResponse<RespondToMatchResult>.Fail(error.Code, error.Message))
            }
        );
    }

    /// <summary>
    /// Get match request inbox (sent and received)
    /// </summary>
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(ApiResponse<CursorPageResponse<MatchRequestDto>>), 200)]
    public async Task<IActionResult> GetInbox([FromQuery] string? status, [FromQuery] string? cursor, [FromQuery] int pageSize = 20)
    {
        var query = new GetMatchInboxQuery(status, cursor, pageSize)
        {
            UserId = User.GetUserId()
        };
        var result = await _mediator.Send(query);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<CursorPageResponse<MatchRequestDto>>.Ok(data)),
            error => StatusCode(500, ApiResponse<CursorPageResponse<MatchRequestDto>>.Fail(error.Code, error.Message))
        );
    }
}
