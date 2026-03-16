using System.Security.Claims;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Loca.Services.Social.Hubs;

[Authorize]
public class VenueChatHub : Hub
{
    private readonly IRedisService _redis;
    private readonly IUserRepository _users;
    private readonly ILogger<VenueChatHub> _logger;
    private readonly LocaDbContext _context;

    public VenueChatHub(
        IRedisService redis,
        IUserRepository users,
        ILogger<VenueChatHub> logger,
        LocaDbContext context)
    {
        _redis = redis;
        _users = users;
        _logger = logger;
        _context = context;
    }

    private Guid GetUserId() => Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {ConnectionId} connected to VenueChatHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {ConnectionId} disconnected from VenueChatHub", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a venue's public chat room
    /// </summary>
    public async Task JoinVenue(Guid venueId)
    {
        var userId = GetUserId();
        var groupName = $"venue_{venueId}";

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await _redis.AddActiveUserAsync(venueId, userId);

        var user = await _users.GetByIdAsync(userId);
        var displayName = user?.DisplayName ?? "Anonymous";

        _logger.LogInformation("User {UserId} joined venue {VenueId}", userId, venueId);

        await Clients.Group(groupName).SendAsync("userJoined", new
        {
            userId,
            displayName,
            joinedAt = DateTime.UtcNow
        });

        // Send recent messages to the joining user
        var recentMessages = await _redis.GetRecentMessagesAsync(venueId);
        if (recentMessages.Count > 0)
        {
            await Clients.Caller.SendAsync("recentMessages", recentMessages.Select(m =>
                JsonSerializer.Deserialize<ChatMessageDto>(m)));
        }
    }

    /// <summary>
    /// Leave a venue's public chat room
    /// </summary>
    public async Task LeaveVenue(Guid venueId)
    {
        var userId = GetUserId();
        var groupName = $"venue_{venueId}";

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await _redis.RemoveActiveUserAsync(venueId, userId);

        _logger.LogInformation("User {UserId} left venue {VenueId}", userId, venueId);

        await Clients.Group(groupName).SendAsync("userLeft", new { userId });
    }

    /// <summary>
    /// Send a message to the venue's public chat
    /// </summary>
    public async Task SendMessage(Guid venueId, string content, string type = "text", string? mediaUrl = null)
    {
        var userId = GetUserId();
        var user = await _users.GetByIdAsync(userId);
        if (user is null) return;

        if (!Enum.TryParse<MessageType>(type, true, out var messageType))
            messageType = MessageType.Text;

        // Persist message
        var message = new ChatMessage
        {
            ChatRoomId = venueId, // Using venueId as chatRoomId for simplicity
            SenderId = userId,
            Type = messageType,
            Content = content,
            MediaUrl = mediaUrl
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        var messageDto = new ChatMessageDto(
            Id: message.Id,
            SenderId: userId,
            SenderName: user.DisplayName,
            SenderPhotoUrl: user.ProfilePhotoUrl,
            Type: type,
            Content: content,
            MediaUrl: mediaUrl,
            SentAt: message.CreatedAt
        );

        // Cache in Redis
        await _redis.CacheMessageAsync(venueId, JsonSerializer.Serialize(messageDto));

        // Broadcast to venue group
        var groupName = $"venue_{venueId}";
        await Clients.Group(groupName).SendAsync("receiveMessage", messageDto);

        _logger.LogInformation("Message sent by {UserId} in venue {VenueId}", userId, venueId);
    }

    /// <summary>
    /// Send typing indicator
    /// </summary>
    public async Task SendTyping(Guid venueId)
    {
        var userId = GetUserId();
        var groupName = $"venue_{venueId}";
        await Clients.OthersInGroup(groupName).SendAsync("userTyping", new { userId });
    }
}
