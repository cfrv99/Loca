namespace Loca.Domain.Enums;

public enum MessageType
{
    Text,
    Image,
    Gif,
    Voice,
    System,
    GameInvite,
    Gift
}

public enum MatchRequestStatus
{
    Pending,
    Accepted,
    Declined,
    Expired
}

public enum ReportReason
{
    Harassment,
    Spam,
    FakeProfile,
    InappropriateContent,
    Other
}

public enum ReportStatus
{
    Pending,
    Reviewed,
    Resolved
}
