using System.Security.Cryptography;
using System.Text;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class CheckInHandler : IRequestHandler<CheckInCommand, Result<CheckInResultDto>>
{
    private readonly IVenueRepository _venues;
    private readonly ICheckInRepository _checkIns;
    private readonly IRedisService _redis;
    private readonly ILogger<CheckInHandler> _logger;

    public CheckInHandler(IVenueRepository venues, ICheckInRepository checkIns, IRedisService redis, ILogger<CheckInHandler> logger)
    {
        _venues = venues;
        _checkIns = checkIns;
        _redis = redis;
        _logger = logger;
    }

    public async Task<Result<CheckInResultDto>> Handle(CheckInCommand cmd, CancellationToken ct)
    {
        // 1. Validate QR
        var venue = await _venues.GetByQrSecretKeyValidatingPayload(cmd.QrPayload, ct);
        if (venue is null)
            return Result<CheckInResultDto>.Failure("INVALID_QR", "QR kodu etibarsızdır və ya vaxtı keçib");

        // 2. Check geofence
        if (!venue.IsWithinGeofence(cmd.Lat, cmd.Lng))
            return Result<CheckInResultDto>.Failure("OUTSIDE_GEOFENCE",
                $"Məkandan kənardasan. {venue.GeofenceRadiusMeters}m daxilində olmalısan.");

        // 3. Rate limit (5 min cooldown)
        var recentCheckIn = await _checkIns.GetRecentAsync(cmd.UserId, venue.Id, TimeSpan.FromMinutes(5), ct);
        if (recentCheckIn is not null)
            return Result<CheckInResultDto>.Failure("RATE_LIMITED", "5 dəqiqə gözləyin");

        // 4. Create check-in
        var checkIn = new CheckIn
        {
            UserId = cmd.UserId,
            VenueId = venue.Id,
            QrPayloadHash = HashPayload(cmd.QrPayload),
            IsAnonymous = cmd.IsAnonymous,
            DeviceFingerprint = cmd.DeviceFingerprint,
            Lat = cmd.Lat,
            Lng = cmd.Lng
        };
        await _checkIns.AddAsync(checkIn, ct);

        // 5. Update Redis counter
        await _redis.IncrementVenueCountAsync(venue.Id, cmd.Gender);
        await _redis.AddActiveUserAsync(venue.Id, cmd.UserId);
        await _redis.AddUserToVenueAsync(cmd.UserId, venue.Id);

        _logger.LogInformation("User {UserId} checked in to venue {VenueId} ({VenueName})", cmd.UserId, venue.Id, venue.Name);

        return Result<CheckInResultDto>.Success(new CheckInResultDto(
            CheckInId: checkIn.Id,
            VenueId: venue.Id,
            VenueName: venue.Name,
            IsAnonymous: cmd.IsAnonymous,
            CheckedInAt: checkIn.CheckInAt
        ));
    }

    private static string HashPayload(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(bytes);
    }
}
