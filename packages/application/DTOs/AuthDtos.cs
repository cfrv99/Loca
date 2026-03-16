namespace Loca.Application.DTOs;

public record GoogleLoginRequest(string IdToken);
public record AppleLoginRequest(string IdentityToken, string AuthorizationCode, string? FullName);
public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User,
    bool IsNewUser
);

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    DateTime DateOfBirth,
    string Gender,
    List<string> Interests,
    List<string> Purposes,
    List<VibePreferenceDto> VibePreferences,
    bool IsOnboarded,
    bool IsPremium,
    int CoinBalance,
    DateTime CreatedAt
);

public record VibePreferenceDto(string Vibe, decimal Weight);

public record PublicUserDto(
    Guid Id,
    string DisplayName,
    string? AvatarUrl,
    int Age,
    string Gender,
    List<string> Interests,
    List<string> Purposes,
    List<PastVenueDto> PastVenues,
    int MemoriesCount,
    string? GiftBadge
);

public record PastVenueDto(Guid VenueId, string VenueName, DateTime LastVisit);

public record OnboardingRequest(
    List<string> Interests,
    List<string> Purposes,
    List<VibePreferenceDto> VibePreferences,
    PrivacySettingsDto PrivacySettings
);

public record PrivacySettingsDto(bool DefaultAnonymous, bool PushEnabled);

public record UpdateProfileRequest(
    string? DisplayName,
    string? Bio,
    List<string>? Interests,
    List<string>? Purposes,
    List<VibePreferenceDto>? VibePreferences
);
