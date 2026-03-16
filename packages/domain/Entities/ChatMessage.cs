using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public string RoomId { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public MessageType MessageType { get; set; }
    public string? Content { get; set; }
    public string? MediaUrl { get; set; }
    public Guid? ReplyToId { get; set; }
    public string? MetadataJson { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public ChatMessage? ReplyTo { get; set; }
    public List<MessageReaction> Reactions { get; set; } = new();
}

public class MessageReaction
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
