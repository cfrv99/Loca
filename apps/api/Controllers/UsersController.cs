using Loca.API.Extensions;
using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Services.Identity.Queries;
using Loca.Services.Social.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loca.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get another user's public profile
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PublicUserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PublicUserDto>), 404)]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(id, User.GetUserId()));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<PublicUserDto>.Ok(data)),
            error => error.Code switch
            {
                "USER_BLOCKED" => StatusCode(403, ApiResponse<PublicUserDto>.Fail(error.Code, error.Message)),
                "USER_NOT_FOUND" => NotFound(ApiResponse<PublicUserDto>.Fail(error.Code, error.Message)),
                _ => StatusCode(500, ApiResponse<PublicUserDto>.Fail(error.Code, error.Message))
            }
        );
    }

    /// <summary>
    /// Report a user
    /// </summary>
    [HttpPost("{id:guid}/report")]
    [ProducesResponseType(typeof(ApiResponse<ReportUserResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ReportUserResult>), 400)]
    public async Task<IActionResult> ReportUser(Guid id, [FromBody] ReportRequest request)
    {
        var cmd = new ReportUserCommand(id, request.Reason, request.Description)
        {
            ReporterId = User.GetUserId()
        };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<ReportUserResult>.Ok(data)),
            error => error.Code switch
            {
                "USER_NOT_FOUND" => NotFound(ApiResponse<ReportUserResult>.Fail(error.Code, error.Message)),
                _ => BadRequest(ApiResponse<ReportUserResult>.Fail(error.Code, error.Message))
            }
        );
    }

    /// <summary>
    /// Block a user
    /// </summary>
    [HttpPost("{id:guid}/block")]
    [ProducesResponseType(typeof(ApiResponse<BlockResponse>), 200)]
    public async Task<IActionResult> BlockUser(Guid id)
    {
        var cmd = new BlockUserCommand(id) { BlockerId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<BlockResponse>.Ok(data)),
            error => BadRequest(ApiResponse<BlockResponse>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Unblock a user
    /// </summary>
    [HttpDelete("{id:guid}/block")]
    [ProducesResponseType(typeof(ApiResponse<BlockResponse>), 200)]
    public async Task<IActionResult> UnblockUser(Guid id)
    {
        var cmd = new UnblockUserCommand(id) { BlockerId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<BlockResponse>.Ok(data)),
            error => BadRequest(ApiResponse<BlockResponse>.Fail(error.Code, error.Message))
        );
    }
}
