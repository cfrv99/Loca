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
    /// Login or register via Google OAuth
    /// </summary>
    [HttpPost("google")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<AuthResponse>.Ok(data)),
            error => BadRequest(ApiResponse<AuthResponse>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Login or register via Apple Sign-In
    /// </summary>
    [HttpPost("apple")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    public async Task<IActionResult> AppleLogin([FromBody] AppleLoginCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<AuthResponse>.Ok(data)),
            error => BadRequest(ApiResponse<AuthResponse>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // TODO: Implement refresh token command
        return Ok(ApiResponse<AuthResponse>.Fail("NOT_IMPLEMENTED", "Refresh token not yet implemented"));
    }

    /// <summary>
    /// Complete onboarding (one-time, sets interests, purposes, vibe preferences)
    /// </summary>
    [HttpPut("onboarding")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingCommand cmd)
    {
        cmd = cmd with { UserId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<UserDto>.Ok(data)),
            error => BadRequest(ApiResponse<UserDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("/api/v1/users/me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetProfileQuery(userId));
        return result.Match<IActionResult>(
            data => Ok(ApiResponse<UserDto>.Ok(data)),
            error => NotFound(ApiResponse<UserDto>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("/api/v1/users/me")]
    [Authorize]
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
}
