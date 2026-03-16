using Loca.Domain.Common;

namespace Loca.Domain.Entities;

public class QrCode : BaseEntity
{
    public Guid VenueId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Venue Venue { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
