namespace Loca.Domain.Enums;

public enum MessageType
{
    Text,
    Emoji,
    Gif,
    Image,
    Voice,
    Gift,
    System
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
    Spam,
    Harassment,
    InappropriateContent,
    FakeProfile,
    Other
}

public enum ChatRoomType
{
    Venue,
    Private,
    Group
}
