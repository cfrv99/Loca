using FluentAssertions;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Commands;
using Loca.Services.Venue.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class CheckInHandlerTests
{
    private readonly IVenueRepository _venues = Substitute.For<IVenueRepository>();
    private readonly ICheckInRepository _checkIns = Substitute.For<ICheckInRepository>();
    private readonly IRedisService _redis = Substitute.For<IRedisService>();
    private readonly ILogger<CheckInHandler> _logger = Substitute.For<ILogger<CheckInHandler>>();
    private readonly CheckInHandler _handler;

    public CheckInHandlerTests()
    {
        _handler = new CheckInHandler(_venues, _checkIns, _redis, _logger);
    }

    [Fact]
    public async Task Should_ReturnError_When_InvalidQr()
    {
        _venues.GetByQrSecretKeyValidatingPayload(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Venue?)null);

        var cmd = new CheckInCommand("invalid-qr", 40.4094, 49.8672, "device-123")
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("INVALID_QR");
    }

    [Fact]
    public async Task Should_ReturnError_When_OutsideGeofence()
    {
        var venue = new Venue
        {
            Name = "Test Venue",
            Latitude = 40.4093,
            Longitude = 49.8671,
            GeofenceRadiusMeters = 150
        };
        _venues.GetByQrSecretKeyValidatingPayload(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(venue);

        var cmd = new CheckInCommand("valid-qr", 40.50, 49.90, "device-123")
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("OUTSIDE_GEOFENCE");
    }

    [Fact]
    public async Task Should_ReturnError_When_RateLimited()
    {
        var venue = new Venue
        {
            Name = "Test Venue",
            Latitude = 40.4093,
            Longitude = 49.8671,
            GeofenceRadiusMeters = 150
        };
        _venues.GetByQrSecretKeyValidatingPayload(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(venue);
        _checkIns.GetRecentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(new CheckIn());

        var cmd = new CheckInCommand("valid-qr", 40.4094, 49.8672, "device-123")
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("RATE_LIMITED");
    }

    [Fact]
    public async Task Should_SuccessfullyCheckIn_When_ValidQrAndInsideGeofence()
    {
        var venue = new Venue
        {
            Name = "Test Venue",
            Latitude = 40.4093,
            Longitude = 49.8671,
            GeofenceRadiusMeters = 150
        };
        _venues.GetByQrSecretKeyValidatingPayload(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(venue);
        _checkIns.GetRecentAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((CheckIn?)null);

        var cmd = new CheckInCommand("valid-qr", 40.4094, 49.8672, "device-123")
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.VenueId.Should().Be(venue.Id);
        result.Value!.VenueName.Should().Be("Test Venue");
    }
}
