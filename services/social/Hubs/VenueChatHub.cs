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

        var venues = await _redis.GetUserVenuesAsync(userGuid);
        foreach (var venueId in venues)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"venue_{venueId}");
            await _redis.RemoveActiveUserAsync(venueId, userGuid);
            await Clients.Group($"venue_{venueId}").SendAsync("userLeft", userId);
        }
        _logger.LogInformation("User {UserId} disconnected from VenueChatHub", userId);
        await base.OnDisconnectedAsync(exception);
    }

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

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userGuid);
        var activeUser = new ActiveUserDto(
            userGuid, user?.DisplayName ?? "Guest", user?.AvatarUrl, user?.GetAge() ?? 0,
            new List<string>(), checkIn.IsAnonymous
        );

        await Clients.Group($"venue_{venueId}").SendAsync("userJoined", activeUser);
        var stats = await _redis.GetVenueStatsAsync(venueGuid);
        await Clients.Caller.SendAsync("activeUsersUpdated", stats);
    }

    public async Task LeaveVenue(string venueId)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"venue_{venueId}");
        await _redis.RemoveActiveUserAsync(Guid.Parse(venueId), Guid.Parse(userId));
        await Clients.Group($"venue_{venueId}").SendAsync("userLeft", userId);
    }

    public async Task SendMessage(string venueId, string content, string type, string? replyToId, object? metadata)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userGuid);

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
            message.Id, userGuid, user?.DisplayName ?? "Guest", user?.AvatarUrl,
            type, content, null, null, metadata, message.CreatedAt
        );

        await _redis.CacheMessageAsync($"venue_{venueId}", dto);
        await Clients.Group($"venue_{venueId}").SendAsync("receiveMessage", dto);
    }

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

        // Get room for this message
        var msg = await _db.Messages.FirstOrDefaultAsync(m => m.Id == msgGuid);
        if (msg is not null)
            await Clients.Group(msg.RoomId).SendAsync("reactionUpdated", messageId, reactions);
    }

    public async Task StartTyping(string venueId)
    {
        var userId = GetUserId();
        var name = Context.User?.FindFirst("name")?.Value ?? "Guest";
        await Clients.OthersInGroup($"venue_{venueId}").SendAsync("typingStarted", userId, name);
    }

    public async Task StopTyping(string venueId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"venue_{venueId}").SendAsync("typingStopped", userId);
    }

    private string GetUserId() => Context.User?.FindFirst("sub")?.Value
        ?? throw new HubException("User not authenticated");
}
