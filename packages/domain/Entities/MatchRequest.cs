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

    // Navigation
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public Conversation? Conversation { get; set; }
}

public class Conversation : BaseEntity
{
    public Guid MatchRequestId { get; set; }
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public DateTime? LastMessageAt { get; set; }

    // Navigation
    public MatchRequest MatchRequest { get; set; } = null!;
    public User User1 { get; set; } = null!;
    public User User2 { get; set; } = null!;
    public List<ChatMessage> Messages { get; set; } = new();
}

public class UserBlock : BaseEntity
{
    public Guid BlockerId { get; set; }
    public Guid BlockedId { get; set; }
    public ReportReason? Reason { get; set; }
    public string? Description { get; set; }

    public User Blocker { get; set; } = null!;
    public User Blocked { get; set; } = null!;
}
