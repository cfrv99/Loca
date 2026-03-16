namespace Loca.Application.DTOs;

public record VenueCardDto(
    Guid Id,
    string Name,
    string? CoverPhotoUrl,
    string Address,
    string Category,
    double DistanceMeters,
    VenueStatsDto Stats,
    string ActivityLevel,
    int ActiveGames,
    int ChatMessageCount
);

public record VenueDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Address,
    string Category,
    double Latitude,
    double Longitude,
    string? CoverPhotoUrl,
    List<string> PhotoUrls,
    decimal? GoogleRating,
    string? Phone,
    string? Website,
    string? WorkingHours,
    VenueStatsDto Stats,
    int GeofenceRadius
);

public record VenueStatsDto(
    int Total,
    int Male,
    int Female
);

public record ActiveUserDto(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    int Age,
    List<string> Interests,
    bool IsAnonymous
);

public record CheckInRequest(
    string QrPayload,
    double Lat,
    double Lng,
    string DeviceFingerprint,
    bool IsAnonymous = false
);

public record CheckInResultDto(
    Guid CheckInId,
    Guid VenueId,
    string VenueName,
    bool IsAnonymous,
    DateTime CheckedInAt
);

public record CheckOutRequest(Guid CheckInId);
