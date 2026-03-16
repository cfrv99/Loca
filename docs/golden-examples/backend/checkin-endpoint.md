# Golden Example: Complete Backend Endpoint

This is the REFERENCE implementation for how every endpoint should look.
Copy this pattern exactly. If your code doesn't look like this, refactor it.

## Controller (API Layer)
```csharp
[ApiController]
[Route("api/v1/venues")]
[Authorize]
public class VenuesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VenuesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get nearby venues with real-time people count
    /// </summary>
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(ApiResponse<CursorPageResponse<VenueCardDto>>), 200)]
    public async Task<IActionResult> GetNearby([FromQuery] GetNearbyVenuesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Match(
            data => Ok(ApiResponse<CursorPageResponse<VenueCardDto>>.Ok(data)),
            error => StatusCode(500, ApiResponse<CursorPageResponse<VenueCardDto>>.Fail(error.Code, error.Message))
        );
    }

    /// <summary>
    /// Check in to a venue via QR code
    /// </summary>
    [HttpPost("checkin")]
    [ProducesResponseType(typeof(ApiResponse<CheckInResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CheckInResultDto>), 400)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInCommand cmd)
    {
        // Inject user ID from JWT
        cmd = cmd with { UserId = User.GetUserId() };
        var result = await _mediator.Send(cmd);
        return result.Match(
            data => Ok(ApiResponse<CheckInResultDto>.Ok(data)),
            error => error.Code switch
            {
                "INVALID_QR" => BadRequest(ApiResponse<CheckInResultDto>.Fail(error.Code, error.Message)),
                "OUTSIDE_GEOFENCE" => BadRequest(ApiResponse<CheckInResultDto>.Fail(error.Code, error.Message)),
                "RATE_LIMITED" => StatusCode(429, ApiResponse<CheckInResultDto>.Fail(error.Code, error.Message)),
                _ => StatusCode(500, ApiResponse<CheckInResultDto>.Fail(error.Code, error.Message))
            }
        );
    }
}
```

## Command + Validator + Handler
```csharp
// ── Command ──
public record CheckInCommand(
    string QrPayload,
    double Lat,
    double Lng,
    string DeviceFingerprint,
    bool IsAnonymous = false
) : IRequest<Result<CheckInResultDto>>
{
    public Guid UserId { get; init; } // Set by controller from JWT
}

// ── Validator ──
public class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    public CheckInCommandValidator()
    {
        RuleFor(x => x.QrPayload).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
        RuleFor(x => x.DeviceFingerprint).NotEmpty().MaximumLength(255);
        RuleFor(x => x.UserId).NotEmpty();
    }
}

// ── Handler ──
public class CheckInHandler : IRequestHandler<CheckInCommand, Result<CheckInResultDto>>
{
    private readonly IVenueRepository _venues;
    private readonly ICheckInRepository _checkIns;
    private readonly IRedisService _redis;
    private readonly ILogger<CheckInHandler> _logger;

    public CheckInHandler(/* DI */) { /* assign */ }

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

        _logger.LogInformation("User {UserId} checked in to venue {VenueId}", cmd.UserId, venue.Id);

        return Result<CheckInResultDto>.Success(new CheckInResultDto(
            CheckInId: checkIn.Id,
            VenueId: venue.Id,
            VenueName: venue.Name,
            IsAnonymous: cmd.IsAnonymous,
            CheckedInAt: checkIn.CheckInAt
        ));
    }
}
```

## Test
```csharp
public class CheckInHandlerTests
{
    [Fact]
    public async Task Should_SuccessfullyCheckIn_When_ValidQrAndInsideGeofence()
    {
        // Arrange
        var venue = CreateTestVenue(lat: 40.4093, lng: 49.8671, radius: 150);
        var cmd = new CheckInCommand("valid-qr", 40.4094, 49.8672, "device-123");
        // ... setup mocks

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.VenueId.Should().Be(venue.Id);
    }

    [Fact]
    public async Task Should_ReturnError_When_OutsideGeofence() { /* ... */ }

    [Fact]
    public async Task Should_ReturnError_When_RateLimited() { /* ... */ }
}
```
