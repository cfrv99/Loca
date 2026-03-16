using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class CheckIn : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid VenueId { get; set; }
    public string QrPayloadHash { get; set; } = string.Empty;
    public DateTime CheckInAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutAt { get; set; }
    public CheckOutReason? CheckOutReason { get; set; }
    public bool IsAnonymous { get; set; }
    public string? DeviceFingerprint { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    // Navigation
    public User? User { get; set; }
    public Venue? Venue { get; set; }

    public bool IsActive => CheckOutAt is null;

    public void CheckOut(CheckOutReason reason)
    {
        CheckOutAt = DateTime.UtcNow;
        CheckOutReason = reason;
    }
}
