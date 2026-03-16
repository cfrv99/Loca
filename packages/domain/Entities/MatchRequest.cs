using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class MatchRequest : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid VenueId { get; set; }
    public string? IntroMessage { get; set; }
    public MatchRequestStatus Status { get; set; } = MatchRequestStatus.Pending;
    public DateTime? RespondedAt { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(48);

    // Navigation
    public User? Sender { get; set; }
    public User? Receiver { get; set; }
    public Conversation? Conversation { get; set; }
}

public class Conversation : BaseEntity
{
    public Guid? MatchRequestId { get; set; }
    public Guid Participant1Id { get; set; }
    public Guid Participant2Id { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
    public int UnreadCount1 { get; set; }
    public int UnreadCount2 { get; set; }
}

public class Block
{
    public Guid BlockerId { get; set; }
    public Guid BlockedId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Report : BaseEntity
{
    public Guid ReporterId { get; set; }
    public Guid ReportedId { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime? ReviewedAt { get; set; }
}
