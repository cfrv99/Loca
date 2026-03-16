namespace Loca.Application.DTOs;

public record ChatMessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    string? SenderAvatar,
    string Type,
    string? Content,
    string? MediaUrl,
    ChatMessageDto? ReplyTo,
    object? Metadata,
    DateTime CreatedAt
);

public record ReactionDto(
    string Emoji,
    int Count,
    List<Guid> UserIds
);

public record GiftEventDto(
    Guid SenderId,
    string SenderName,
    string GiftName,
    string GiftTier,
    string? AnimationUrl,
    int CoinCost
);

public record PrivateMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string? SenderAvatar,
    string Type,
    string? Content,
    string? MediaUrl,
    DateTime CreatedAt
);

public record ConversationDto(
    Guid ConversationId,
    PublicUserDto OtherUser,
    ChatMessageDto? LastMessage,
    int UnreadCount,
    DateTime UpdatedAt
);

public record MatchRequestDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    string? SenderAvatar,
    string? IntroMessage,
    Guid VenueId,
    string Status,
    DateTime CreatedAt
);
