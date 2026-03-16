using Loca.Domain.Common;

namespace Loca.Domain.Entities;

public class ChatRoom : BaseEntity
{
    public string RoomId { get; set; } = string.Empty;
    public Guid? VenueId { get; set; }
    public string RoomType { get; set; } = "venue"; // venue or private
}
