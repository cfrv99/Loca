using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Social.Hubs;

/// <summary>
/// PrivateChatHub — /hubs/private-chat
/// Handles 1-on-1 private messaging between matched users.
/// Auth: JWT Bearer required.
/// </summary>
[Authorize]
public class PrivateChatHub : Hub
{
    private readonly IRedisService _redis;
    private readonly LocaDbContext _db;
    private readonly ILogger<PrivateChatHub> _logger;

    public PrivateChatHub(IRedisService redis, LocaDbContext db, ILogger<PrivateChatHub> logger)
    {
        _redis = redis;
        _db = db;
        _logger = logger;
    }

    // ── Connection lifecycle ──

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);

        // Set user online
        await _redis.SetUserOnlineAsync(userGuid, true);

        // Join all conversation groups this user participates in
        var conversations = await _db.Conversations
            .Where(c => c.Participant1Id == userGuid || c.Participant2Id == userGuid)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var conversationId in conversations)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"dm_{conversationId}");
            await _redis.AddUserToConversationAsync(userGuid, conversationId);
        }

        // Broadcast online status to all conversation partners
        await BroadcastOnlineStatusAsync(userGuid, true);

        _logger.LogInformation("User {UserId} connected to PrivateChatHub, joined {Count} conversations",
            userId, conversations.Count);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);

        // Set user offline with last seen timestamp
        await _redis.SetUserOnlineAsync(userGuid, false);

        // Leave all conversation groups
        var conversations = await _redis.GetUserConversationsAsync(userGuid);
        foreach (var conversationId in conversations)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"dm_{conversationId}");
            await _redis.RemoveUserFromConversationAsync(userGuid, conversationId);
        }

        // Broadcast offline status with last seen
        await BroadcastOnlineStatusAsync(userGuid, false);

        _logger.LogInformation("User {UserId} disconnected from PrivateChatHub", userId);
        await base.OnDisconnectedAsync(exception);
    }

    // ── Client → Server methods (PascalCase) ──

    /// <summary>
    /// Send a private message in a conversation.
    /// Validates conversation exists and user is a participant.
    /// </summary>
    public async Task SendPrivateMessage(string conversationId, string content, string type, object? metadata)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);
        var convGuid = Guid.Parse(conversationId);

        // Validate conversation and participation
        var conversation = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == convGuid);
        if (conversation is null)
        {
            await Clients.Caller.SendAsync("error", new { code = "CONVERSATION_NOT_FOUND", message = "Söhbət tapılmadı" });
            return;
        }

        if (conversation.Participant1Id != userGuid && conversation.Participant2Id != userGuid)
        {
            await Clients.Caller.SendAsync("error", new { code = "NOT_PARTICIPANT", message = "Bu söhbətdə iştirak etmirsiniz" });
            return;
        }

        // Check if user is blocked by the other participant
        var otherUserId = conversation.Participant1Id == userGuid
            ? conversation.Participant2Id
            : conversation.Participant1Id;

        var isBlocked = await _db.Blocks.AnyAsync(b =>
            (b.BlockerId == otherUserId && b.BlockedId == userGuid) ||
            (b.BlockerId == userGuid && b.BlockedId == otherUserId));

        if (isBlocked)
        {
            await Clients.Caller.SendAsync("error", new { code = "USER_BLOCKED", message = "Bu istifadəçi ilə əlaqə yaratmaq mümkün deyil" });
            return;
        }

        // Validate content
        if (string.IsNullOrWhiteSpace(content) && type == "text")
        {
            await Clients.Caller.SendAsync("error", new { code = "EMPTY_MESSAGE", message = "Mesaj boş ola bilməz" });
            return;
        }

        // Persist message
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userGuid);
        var message = new ChatMessage
        {
            RoomId = $"dm_{conversationId}",
            SenderId = userGuid,
            MessageType = Enum.Parse<MessageType>(type, true),
            Content = content,
        };

        _db.Messages.Add(message);

        // Update conversation metadata
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.LastMessagePreview = content?.Length > 100 ? content[..100] : content;

        // Increment unread count for the other participant
        if (conversation.Participant1Id == userGuid)
            conversation.UnreadCount2++;
        else
            conversation.UnreadCount1++;

        await _db.SaveChangesAsync();

        // Build DTO and broadcast
        var dto = new PrivateMessageDto(
            message.Id,
            convGuid,
            userGuid,
            user?.DisplayName ?? "Guest",
            user?.AvatarUrl,
            type,
            content,
            null,
            message.CreatedAt
        );

        // Cache in Redis
        var chatDto = new ChatMessageDto(
            message.Id, userGuid, user?.DisplayName ?? "Guest", user?.AvatarUrl,
            type, content, null, null, metadata, message.CreatedAt
        );
        await _redis.CacheMessageAsync($"dm_{conversationId}", chatDto);

        // Broadcast to both participants in the conversation group
        await Clients.Group($"dm_{conversationId}").SendAsync("receivePrivateMessage", dto);

        _logger.LogInformation("Private message sent in conversation {ConversationId} by {UserId}",
            conversationId, userId);
    }

    /// <summary>
    /// Mark messages as read up to a given message ID.
    /// Resets unread count and broadcasts read receipt to other participant.
    /// </summary>
    public async Task MarkRead(string conversationId, string messageId)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);
        var convGuid = Guid.Parse(conversationId);

        var conversation = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == convGuid);
        if (conversation is null) return;

        // Reset unread count for this user
        if (conversation.Participant1Id == userGuid)
            conversation.UnreadCount1 = 0;
        else if (conversation.Participant2Id == userGuid)
            conversation.UnreadCount2 = 0;
        else
            return; // Not a participant

        await _db.SaveChangesAsync();

        // Broadcast read receipt to the other participant
        await Clients.OthersInGroup($"dm_{conversationId}")
            .SendAsync("messagesRead", conversationId, userId, messageId);

        _logger.LogInformation("Messages marked read in conversation {ConversationId} by {UserId} up to {MessageId}",
            conversationId, userId, messageId);
    }

    /// <summary>
    /// Broadcast typing indicator to other participant in conversation.
    /// Client should debounce this (3s recommended).
    /// </summary>
    public async Task StartTyping(string conversationId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"dm_{conversationId}")
            .SendAsync("typingStarted", conversationId, userId);
    }

    /// <summary>
    /// Clear typing indicator for other participant.
    /// </summary>
    public async Task StopTyping(string conversationId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"dm_{conversationId}")
            .SendAsync("typingStopped", conversationId, userId);
    }

    // ── Private helpers ──

    private async Task BroadcastOnlineStatusAsync(Guid userId, bool isOnline)
    {
        var conversations = await _db.Conversations
            .Where(c => c.Participant1Id == userId || c.Participant2Id == userId)
            .ToListAsync();

        var lastSeen = isOnline ? (DateTime?)null : DateTime.UtcNow;

        foreach (var conv in conversations)
        {
            var otherUserId = conv.Participant1Id == userId
                ? conv.Participant2Id
                : conv.Participant1Id;

            // Send to the other user's notification group since they may not be in the DM group
            await Clients.Group($"dm_{conv.Id}")
                .SendAsync("userOnlineStatusChanged", userId.ToString(), isOnline, lastSeen?.ToString("O"));
        }
    }

    private string GetUserId() => Context.User?.FindFirst("sub")?.Value
        ?? throw new HubException("User not authenticated");
}
