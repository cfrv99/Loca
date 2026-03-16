namespace Loca.Application.DTOs;

public record RegisterDeviceTokenRequest(
    string Token,
    string Platform // "ios" or "android"
);

public record DeviceTokenDto(
    Guid Id,
    string Token,
    string Platform,
    bool IsActive
);

public record NotificationDto(
    Guid Id,
    string Type,
    string? Title,
    string? Body,
    object? Data,
    bool IsRead,
    DateTime CreatedAt
);

public record MatchNotificationDto(
    Guid RequestId,
    Guid SenderId,
    string SenderName,
    string? SenderAvatar,
    string? IntroMessage,
    Guid VenueId
);

public record MatchAcceptedNotificationDto(
    Guid ConversationId,
    Guid UserId,
    string UserName
);

public record GiftNotificationDto(
    Guid SenderId,
    string SenderName,
    string GiftName,
    string GiftTier,
    string? AnimationUrl,
    int CoinCost,
    string Context // "public_chat" or "private_chat"
);

public record GameStartingNotificationDto(
    Guid SessionId,
    string GameType,
    string VenueName,
    Guid VenueId
);
