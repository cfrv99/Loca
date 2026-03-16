using Loca.API.Extensions;
using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Services.Identity.Commands;
using Loca.Services.Identity.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loca.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Login or register with Google OAuth
    /// </summary>
    [HttpPost("google")]
    [ProducesResponseType(typeof(ApiResponse<LoginResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResultDto>), 400)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<LoginResultDto>.Ok(data)),
            error => BadRequest(ApiResponse<LoginResultDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Login or register with Apple Sign-In
    /// </summary>
    [HttpPost("apple")]
    [ProducesResponseType(typeof(ApiResponse<LoginResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResultDto>), 400)]
    public async Task<IActionResult> AppleLogin([FromBody] AppleLoginCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<LoginResultDto>.Ok(data)),
            error => BadRequest(ApiResponse<LoginResultDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenDto>), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<RefreshTokenDto>.Ok(data)),
            error => Unauthorized(ApiResponse<RefreshTokenDto>.Fail(error.Code, error.Message))
        );
    }
}

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _mediator.Send(new GetMyProfileQuery(User.GetUserId()));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<UserDto>.Ok(data)),
            error => NotFound(ApiResponse<UserDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand cmd)
    {
        cmd = cmd with { UserId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<UserDto>.Ok(data)),
            error => BadRequest(ApiResponse<UserDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get another user's public profile
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), 200)]
    public async Task<IActionResult> GetUserProfile(Guid userId)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(User.GetUserId(), userId));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<UserProfileDto>.Ok(data)),
            error => NotFound(ApiResponse<UserProfileDto>.Fail(error.Code, error.Message))
        );
    }
}
