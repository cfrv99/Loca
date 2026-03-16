using Loca.Application.DTOs;

namespace Loca.Application.Interfaces;

/// <summary>
/// Service for sending real-time notifications via SignalR NotificationHub
/// and managing push notification device tokens.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send match request notification to receiver via SignalR.
    /// </summary>
    Task SendMatchRequestNotificationAsync(Guid receiverId, MatchNotificationDto notification);

    /// <summary>
    /// Send match accepted notification to the original sender via SignalR.
    /// </summary>
    Task SendMatchAcceptedNotificationAsync(Guid userId, MatchAcceptedNotificationDto notification);

    /// <summary>
    /// Send gift received notification to recipient via SignalR.
    /// </summary>
    Task SendGiftNotificationAsync(Guid recipientId, GiftNotificationDto notification);

    /// <summary>
    /// Send game starting notification to all checked-in users at a venue via SignalR.
    /// </summary>
    Task SendGameStartingNotificationAsync(Guid venueId, GameStartingNotificationDto notification);

    /// <summary>
    /// Register a device token for push notifications (FCM/APNs).
    /// </summary>
    Task RegisterDeviceTokenAsync(Guid userId, string token, string platform);

    /// <summary>
    /// Log a notification for audit trail.
    /// </summary>
    Task LogNotificationAsync(Guid userId, string type, string? title, string? body, object? data);
}
