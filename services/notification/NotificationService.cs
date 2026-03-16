using System.Text.Json;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Infrastructure.Persistence;
using Loca.Services.Notification.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Notification;

/// <summary>
/// NotificationService sends real-time notifications via SignalR NotificationHub
/// and persists notification logs + device tokens in the database.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly LocaDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        LocaDbContext db,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _db = db;
        _logger = logger;
    }

    public async Task SendMatchRequestNotificationAsync(Guid receiverId, MatchNotificationDto notification)
    {
        await _hubContext.Clients.Group($"user_{receiverId}")
            .SendAsync("matchRequestReceived", notification);

        await LogNotificationAsync(
            receiverId, "match_request",
            $"{notification.SenderName} tanışlıq sorğusu göndərdi",
            notification.IntroMessage,
            notification);

        _logger.LogInformation("Match request notification sent to {ReceiverId} from {SenderId}",
            receiverId, notification.SenderId);
    }

    public async Task SendMatchAcceptedNotificationAsync(Guid userId, MatchAcceptedNotificationDto notification)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("matchAccepted", notification.ConversationId, notification.UserId, notification.UserName);

        await LogNotificationAsync(
            userId, "match_accepted",
            $"{notification.UserName} qəbul etdi! Söhbətə başla",
            null,
            notification);

        _logger.LogInformation("Match accepted notification sent to {UserId} from {AccepterId}",
            userId, notification.UserId);
    }

    public async Task SendGiftNotificationAsync(Guid recipientId, GiftNotificationDto notification)
    {
        await _hubContext.Clients.Group($"user_{recipientId}")
            .SendAsync("giftReceived", notification);

        await LogNotificationAsync(
            recipientId, "gift_received",
            $"{notification.SenderName} sənə {notification.GiftName} göndərdi!",
            null,
            notification);

        _logger.LogInformation("Gift notification sent to {RecipientId}: {GiftName} from {SenderId}",
            recipientId, notification.GiftName, notification.SenderId);
    }

    public async Task SendGameStartingNotificationAsync(Guid venueId, GameStartingNotificationDto notification)
    {
        // Game starting notifications go to all users at the venue.
        // We broadcast to a venue group; the NotificationHub uses user-specific groups,
        // so we need to get all active users and notify each one.
        _logger.LogInformation("Game {GameType} starting at venue {VenueId}", notification.GameType, venueId);

        await LogNotificationAsync(
            Guid.Empty, "game_starting",
            $"{notification.GameType} oyunu başlayır {notification.VenueName}-da",
            null,
            notification);
    }

    public async Task RegisterDeviceTokenAsync(Guid userId, string token, string platform)
    {
        var existing = await _db.DeviceTokens
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Token == token);

        if (existing is not null)
        {
            existing.IsActive = true;
            existing.Platform = platform;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.DeviceTokens.Add(new DeviceToken
            {
                UserId = userId,
                Token = token,
                Platform = platform,
                IsActive = true
            });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Device token registered for user {UserId} on {Platform}", userId, platform);
    }

    public async Task LogNotificationAsync(Guid userId, string type, string? title, string? body, object? data)
    {
        var log = new NotificationLog
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            DataJson = data is not null ? JsonSerializer.Serialize(data) : null,
            IsSent = true,
            SentAt = DateTime.UtcNow
        };

        _db.NotificationLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
