using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string? GiftId { get; set; }
    public bool IsModerated { get; set; }

    // Navigation
    public ChatRoom ChatRoom { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
