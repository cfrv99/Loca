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

    public CheckInHandler(
        IVenueRepository venues,
        ICheckInRepository checkIns,
        IRedisService redis,
        ILogger<CheckInHandler> logger)
    {
        _venues = venues;
        _checkIns = checkIns;
        _redis = redis;
        _logger = logger;
    }

    public async Task<Result<CheckInResultDto>> Handle(CheckInCommand cmd, CancellationToken ct)
    {
        // 1. Validate QR
        var venue = await _venues.GetByQrPayloadAsync(cmd.QrPayload, ct);
        if (venue is null)
            return Result<CheckInResultDto>.Failure("INVALID_QR", "QR code is invalid or expired");

        // 2. Check geofence
        if (!venue.IsWithinGeofence(cmd.Lat, cmd.Lng))
            return Result<CheckInResultDto>.Failure("OUTSIDE_GEOFENCE",
                $"You must be within {venue.GeofenceRadiusMeters}m of the venue");

        // 3. Rate limit
        var recentCheckIn = await _checkIns.GetRecentAsync(cmd.UserId, venue.Id, TimeSpan.FromMinutes(5), ct);
        if (recentCheckIn is not null)
            return Result<CheckInResultDto>.Failure("RATE_LIMITED", "Wait 5 minutes before checking in again");

        // 4. Create check-in
        var checkIn = new CheckIn
        {
            UserId = cmd.UserId,
            VenueId = venue.Id,
            QrPayloadHash = HashQrPayload(cmd.QrPayload),
            IsAnonymous = cmd.IsAnonymous,
            DeviceFingerprint = cmd.DeviceFingerprint,
            Lat = cmd.Lat,
            Lng = cmd.Lng
        };
        await _checkIns.AddAsync(checkIn, ct);

        // 5. Update Redis counter
        await _redis.IncrementVenueCountAsync(venue.Id);
        await _redis.AddActiveUserAsync(venue.Id, cmd.UserId);

        _logger.LogInformation("User {UserId} checked in to venue {VenueId}", cmd.UserId, venue.Id);

        return Result<CheckInResultDto>.Success(new CheckInResultDto(
            CheckInId: checkIn.Id,
            VenueId: venue.Id,
            VenueName: venue.Name,
            IsAnonymous: cmd.IsAnonymous,
            CheckedInAt: checkIn.CheckInAt
        ));
    }

    private static string HashQrPayload(string payload)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}
