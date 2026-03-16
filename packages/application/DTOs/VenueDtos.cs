namespace Loca.Application.DTOs;

public record VenueCardDto(
    Guid Id,
    string Name,
    string Address,
    string Category,
    string? CoverPhotoUrl,
    double Latitude,
    double Longitude,
    double DistanceMeters,
    int ActiveCount,
    int MaleCount,
    int FemaleCount,
    string ActivityLevel
);

public record VenueDetailDto(
    Guid Id,
    string Name,
    string Description,
    string Address,
    string Category,
    string? CoverPhotoUrl,
    List<string> PhotoUrls,
    string? Phone,
    string? Website,
    string? InstagramHandle,
    double Latitude,
    double Longitude,
    int GeofenceRadiusMeters,
    string? OpeningHours,
    int ActiveCount,
    bool IsVerified,
    DateTime CreatedAt
);

public record CheckInResultDto(
    Guid CheckInId,
    Guid VenueId,
    string VenueName,
    bool IsAnonymous,
    DateTime CheckedInAt
);

public record QrCodeDto(
    string Payload,
    DateTime ExpiresAt,
    int RotationSeconds
);
