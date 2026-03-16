using Loca.Domain.Common;

namespace Loca.Domain.Entities;

public class DeviceToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // "ios" or "android"
    public bool IsActive { get; set; } = true;

    // Navigation
    public User? User { get; set; }
}

public class NotificationLog : BaseEntity
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DataJson { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsRead { get; set; }
}
