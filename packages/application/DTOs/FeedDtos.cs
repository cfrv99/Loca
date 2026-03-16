namespace Loca.Application.DTOs;

public record PostDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatar,
    string? Content,
    List<string> MediaUrls,
    int LikeCount,
    int CommentCount,
    bool IsLikedByMe,
    DateTime CreatedAt
);

public record CommentDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatar,
    string Content,
    DateTime CreatedAt
);

public record CreatePostRequest(
    Guid VenueId,
    string? Content,
    List<string>? MediaUrls
);

public record AddCommentRequest(
    string Content
);

public record ToggleLikeResponse(
    bool Liked,
    int LikeCount
);

public record ReportRequest(
    string Reason,
    string? Description
);

public record BlockResponse(
    bool Blocked
);

public record SendMatchRequest(
    Guid ReceiverId,
    string? IntroMessage
);

public record RespondToMatchRequest(
    string Action // "accept" or "decline"
);
