using Loca.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Notification.Hubs;

/// <summary>
/// NotificationHub — /hubs/notifications
/// Auto-connects on app start. Delivers real-time notifications to users.
/// Server → Client only (no client→server methods needed for MVP).
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly IRedisService _redis;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(IRedisService redis, ILogger<NotificationHub> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        // Join user-specific notification group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await _redis.SetUserOnlineAsync(Guid.Parse(userId), true);

        _logger.LogInformation("User {UserId} connected to NotificationHub", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        await _redis.SetUserOnlineAsync(Guid.Parse(userId), false);

        _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);
        await base.OnDisconnectedAsync(exception);
    }

    private string GetUserId() => Context.User?.FindFirst("sub")?.Value
        ?? throw new HubException("User not authenticated");
}
