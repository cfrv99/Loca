using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class ChatRoom : BaseEntity
{
    public Guid? VenueId { get; set; }
    public ChatRoomType Type { get; set; }
    public string? Name { get; set; }

    // Navigation
    public Venue? Venue { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
}
