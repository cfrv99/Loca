namespace Loca.Application.DTOs;

public record ChatMessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    string? SenderPhotoUrl,
    string Type,
    string Content,
    string? MediaUrl,
    DateTime SentAt
);

public record SendMessageDto(
    Guid ChatRoomId,
    string Type,
    string Content,
    string? MediaUrl
);

public record ActiveUserDto(
    Guid UserId,
    string DisplayName,
    string? ProfilePhotoUrl,
    string? Bio,
    List<string> Interests,
    bool IsAnonymous
);
