using Loca.API.Extensions;
using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loca.API.Controllers;

/// <summary>
/// Notification endpoints for device token registration and notification management.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Register FCM/APNs device token for push notifications.
    /// Called on app login and refreshed on each app open.
    /// </summary>
    [HttpPost("device-token")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(ApiResponse<object>.Fail("INVALID_TOKEN", "Token boş ola bilməz"));

        if (request.Platform != "ios" && request.Platform != "android")
            return BadRequest(ApiResponse<object>.Fail("INVALID_PLATFORM", "Platform 'ios' və ya 'android' olmalıdır"));

        var userId = User.GetUserId();

        await _notificationService.RegisterDeviceTokenAsync(userId, request.Token, request.Platform);

        return Ok(ApiResponse<object>.Ok(new { registered = true }));
    }
}
