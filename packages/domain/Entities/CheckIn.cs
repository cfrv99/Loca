using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class CheckIn : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid VenueId { get; set; }
    public string QrPayloadHash { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public CheckInStatus Status { get; set; } = CheckInStatus.Active;
    public DateTime CheckInAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
}
