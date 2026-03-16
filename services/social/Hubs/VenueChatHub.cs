using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Social.Hubs;

/// <summary>
/// VenueChatHub — /hubs/venue-chat
/// Public chat for venue rooms. Requires active check-in to join.
/// Auth: JWT Bearer required.
/// </summary>
[Authorize]
public class VenueChatHub : Hub
{
    private readonly IRedisService _redis;
    private readonly ICheckInRepository _checkIns;
    private readonly LocaDbContext _db;
    private readonly ILogger<VenueChatHub> _logger;

    public VenueChatHub(IRedisService redis, ICheckInRepository checkIns, LocaDbContext db, ILogger<VenueChatHub> logger)
    {
        _redis = redis;
        _checkIns = checkIns;
        _db = db;
        _logger = logger;
    }

    // ── Connection lifecycle ──

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await _redis.SetUserOnlineAsync(Guid.Parse(userId), true);
        _logger.LogInformation("User {UserId} connected to VenueChatHub", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);
        await _redis.SetUserOnlineAsync(userGuid, false);

        // Remove from all venue groups and update active user sets
        var venues = await _redis.GetUserVenuesAsync(userGuid);
        foreach (var venueId in venues)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"venue_{venueId}");
            await _redis.RemoveActiveUserAsync(venueId, userGuid);
            await _redis.RemoveUserFromVenueAsync(userGuid, venueId);
            await Clients.Group($"venue_{venueId}").SendAsync("userLeft", userId);

            // Update live stats for remaining users
            var stats = await _redis.GetVenueStatsAsync(venueId);
            await Clients.Group($"venue_{venueId}").SendAsync("activeUsersUpdated", stats);
        }

        _logger.LogInformation("User {UserId} disconnected from VenueChatHub", userId);
        await base.OnDisconnectedAsync(exception);
    }

    // ── Client → Server methods (PascalCase) ──

    /// <summary>
    /// Join a venue chat room. Requires active check-in.
    /// Broadcasts userJoined to room members.
    /// </summary>
    public async Task JoinVenue(string venueId)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);
        var venueGuid = Guid.Parse(venueId);

        // Verify active check-in
        var checkIn = await _checkIns.GetActiveCheckInAsync(userGuid, venueGuid);
        if (checkIn is null)
        {
            await Clients.Caller.SendAsync("error", new { code = "NOT_CHECKED_IN", message = "Əvvəlcə QR scan edin" });
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"venue_{venueId}");
        await _redis.AddActiveUserAsync(venueGuid, userGuid);
        await _redis.AddUserToVenueAsync(userGuid, venueGuid);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userGuid);

        // Handle anonymous mode: use "Guest_XXXX" display name
        var displayName = checkIn.IsAnonymous
            ? $"Guest_{userGuid.ToString()[..4].ToUpper()}"
            : (user?.DisplayName ?? "Guest");

        var activeUser = new ActiveUserDto(
            userGuid, displayName, checkIn.IsAnonymous ? null : user?.AvatarUrl,
            user?.GetAge() ?? 0, new List<string>(), checkIn.IsAnonymous
        );

        await Clients.Group($"venue_{venueId}").SendAsync("userJoined", activeUser);

        // Send current active users count to the joining user
        var stats = await _redis.GetVenueStatsAsync(venueGuid);
        await Clients.Caller.SendAsync("activeUsersUpdated", stats);

        _logger.LogInformation("User {UserId} joined venue {VenueId} chat (anonymous={IsAnonymous})",
            userId, venueId, checkIn.IsAnonymous);
    }

    /// <summary>
    /// Leave a venue chat room. Broadcasts userLeft.
    /// </summary>
    public async Task LeaveVenue(string venueId)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);
        var venueGuid = Guid.Parse(venueId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"venue_{venueId}");
        await _redis.RemoveActiveUserAsync(venueGuid, userGuid);
        await _redis.RemoveUserFromVenueAsync(userGuid, venueGuid);
        await Clients.Group($"venue_{venueId}").SendAsync("userLeft", userId);

        // Update stats for remaining users
        var stats = await _redis.GetVenueStatsAsync(venueGuid);
        await Clients.Group($"venue_{venueId}").SendAsync("activeUsersUpdated", stats);
    }

    /// <summary>
    /// Send a chat message. Types: text, image, gif, voice.
    /// Server validates, persists, and broadcasts.
    /// </summary>
    public async Task SendMessage(string venueId, string content, string type, string? replyToId, object? metadata)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);

        // Validate content
        if (string.IsNullOrWhiteSpace(content) && type == "text")
        {
            await Clients.Caller.SendAsync("error", new { code = "EMPTY_MESSAGE", message = "Mesaj boş ola bilməz" });
            return;
        }

        if (content?.Length > 1000)
        {
            await Clients.Caller.SendAsync("error", new { code = "MESSAGE_TOO_LONG", message = "Mesaj 1000 simvoldan çox ola bilməz" });
            return;
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userGuid);

        // Check if user is anonymous at this venue
        var venueGuid = Guid.Parse(venueId);
        var checkIn = await _checkIns.GetActiveCheckInAsync(userGuid, venueGuid);
        var isAnonymous = checkIn?.IsAnonymous ?? false;

        var senderName = isAnonymous
            ? $"Guest_{userGuid.ToString()[..4].ToUpper()}"
            : (user?.DisplayName ?? "Guest");

        var message = new ChatMessage
        {
            RoomId = $"venue_{venueId}",
            SenderId = userGuid,
            MessageType = Enum.Parse<MessageType>(type, true),
            Content = content,
            ReplyToId = replyToId != null ? Guid.Parse(replyToId) : null,
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        var dto = new ChatMessageDto(
            message.Id, userGuid, senderName, isAnonymous ? null : user?.AvatarUrl,
            type, content, null, null, metadata, message.CreatedAt
        );

        await _redis.CacheMessageAsync($"venue_{venueId}", dto);
        await Clients.Group($"venue_{venueId}").SendAsync("receiveMessage", dto);
    }

    /// <summary>
    /// Add/toggle reaction on a message. Supported emoji: heart, laugh, wow, clap, fire, sad.
    /// </summary>
    public async Task SendReaction(string messageId, string emoji)
    {
        var userId = Guid.Parse(GetUserId());
        var msgGuid = Guid.Parse(messageId);

        var existing = await _db.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == msgGuid && r.UserId == userId && r.Emoji == emoji);

        if (existing is not null)
            _db.MessageReactions.Remove(existing);
        else
            _db.MessageReactions.Add(new MessageReaction { MessageId = msgGuid, UserId = userId, Emoji = emoji });

        await _db.SaveChangesAsync();

        var reactions = await _db.MessageReactions
            .Where(r => r.MessageId == msgGuid)
            .GroupBy(r => r.Emoji)
            .Select(g => new ReactionDto(g.Key, g.Count(), g.Select(r => r.UserId).ToList()))
            .ToListAsync();

        // Get room for this message to broadcast
        var msg = await _db.Messages.FirstOrDefaultAsync(m => m.Id == msgGuid);
        if (msg is not null)
            await Clients.Group(msg.RoomId).SendAsync("reactionUpdated", messageId, reactions);
    }

    /// <summary>
    /// Pin a message in venue chat. Only venue admins (owner) can pin.
    /// </summary>
    public async Task PinMessage(string venueId, string messageId)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);
        var venueGuid = Guid.Parse(venueId);
        var msgGuid = Guid.Parse(messageId);

        // Verify venue ownership (admin check)
        var venue = await _db.Venues.FirstOrDefaultAsync(v => v.Id == venueGuid);
        if (venue is null || venue.OwnerUserId != userGuid)
        {
            await Clients.Caller.SendAsync("error", new { code = "NOT_VENUE_ADMIN", message = "Yalnız məkan admini mesaj sabitləyə bilər" });
            return;
        }

        // Unpin any previously pinned message in this room
        var previouslyPinned = await _db.Messages
            .Where(m => m.RoomId == $"venue_{venueId}" && m.IsPinned)
            .ToListAsync();

        foreach (var pm in previouslyPinned)
            pm.IsPinned = false;

        // Pin the new message
        var message = await _db.Messages.FirstOrDefaultAsync(m => m.Id == msgGuid);
        if (message is null || message.RoomId != $"venue_{venueId}")
        {
            await Clients.Caller.SendAsync("error", new { code = "MESSAGE_NOT_FOUND", message = "Mesaj tapılmadı" });
            return;
        }

        message.IsPinned = true;
        await _db.SaveChangesAsync();

        // Get sender info for the pinned message DTO
        var sender = await _db.Users.FirstOrDefaultAsync(u => u.Id == message.SenderId);
        var dto = new ChatMessageDto(
            message.Id, message.SenderId, sender?.DisplayName ?? "Guest", sender?.AvatarUrl,
            message.MessageType.ToString().ToLower(), message.Content, message.MediaUrl,
            null, null, message.CreatedAt
        );

        await Clients.Group($"venue_{venueId}").SendAsync("messagePinned", dto);
        _logger.LogInformation("Message {MessageId} pinned in venue {VenueId} by admin {UserId}",
            messageId, venueId, userId);
    }

    /// <summary>
    /// Broadcast typing indicator to others in venue room.
    /// Client should debounce this (3s recommended).
    /// </summary>
    public async Task StartTyping(string venueId)
    {
        var userId = GetUserId();
        var name = Context.User?.FindFirst("name")?.Value ?? "Guest";
        await Clients.OthersInGroup($"venue_{venueId}").SendAsync("typingStarted", userId, name);
    }

    /// <summary>
    /// Clear typing indicator.
    /// </summary>
    public async Task StopTyping(string venueId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"venue_{venueId}").SendAsync("typingStopped", userId);
    }

    // ── Server-only broadcast methods (called by other services) ──

    /// <summary>
    /// Broadcast gift received event to all users in venue chat.
    /// Called by the Economy service when a gift is sent in public chat.
    /// This is a static helper that other services can use via IHubContext.
    /// </summary>
    public static async Task BroadcastGiftReceivedAsync(
        IHubContext<VenueChatHub> hubContext,
        string venueId,
        GiftEventDto gift)
    {
        await hubContext.Clients.Group($"venue_{venueId}")
            .SendAsync("giftReceived", gift);
    }

    /// <summary>
    /// Broadcast a system message to venue chat (e.g., game results).
    /// Called by the Game service when a game ends.
    /// </summary>
    public static async Task BroadcastSystemMessageAsync(
        IHubContext<VenueChatHub> hubContext,
        string venueId,
        string content)
    {
        var systemMessage = new ChatMessageDto(
            Guid.NewGuid(),
            Guid.Empty,
            "Sistem",
            null,
            "system",
            content,
            null,
            null,
            null,
            DateTime.UtcNow
        );

        await hubContext.Clients.Group($"venue_{venueId}")
            .SendAsync("receiveMessage", systemMessage);
    }

    private string GetUserId() => Context.User?.FindFirst("sub")?.Value
        ?? throw new HubException("User not authenticated");
}
