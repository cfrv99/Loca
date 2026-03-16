namespace Loca.Application.DTOs;

public record LoginResultDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User,
    bool IsNewUser
);

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? FirstName,
    string? LastName,
    string? ProfilePhotoUrl,
    string? Bio,
    bool IsOnboardingComplete,
    DateTime CreatedAt
);

public record UserProfileDto(
    Guid Id,
    string DisplayName,
    string? ProfilePhotoUrl,
    string? Bio,
    List<string> Interests,
    int TotalCheckIns,
    int TotalGamesPlayed,
    int TotalGiftsReceived,
    int TotalMatchesMade
);

public record UpdateProfileDto(
    string? DisplayName,
    string? FirstName,
    string? LastName,
    string? Bio,
    DateTime? DateOfBirth,
    string? Gender,
    List<string>? Interests,
    List<string>? Purposes,
    string? VibePreference,
    string? PrivacyLevel
);

public record RefreshTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
