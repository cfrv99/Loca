using FluentAssertions;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Commands;
using Loca.Services.Venue.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NetTopologySuite.Geometries;

namespace Loca.Tests.Unit.Services;

public class CheckInHandlerTests
{
    private readonly IVenueRepository _venueRepo;
    private readonly ICheckInRepository _checkInRepo;
    private readonly IRedisService _redis;
    private readonly ILogger<CheckInHandler> _logger;
    private readonly CheckInHandler _handler;

    public CheckInHandlerTests()
    {
        _venueRepo = Substitute.For<IVenueRepository>();
        _checkInRepo = Substitute.For<ICheckInRepository>();
        _redis = Substitute.For<IRedisService>();
        _logger = Substitute.For<ILogger<CheckInHandler>>();
        _handler = new CheckInHandler(_venueRepo, _checkInRepo, _redis, _logger);
    }

    [Fact]
    public async Task Should_SuccessfullyCheckIn_When_ValidQrAndInsideGeofence()
    {
        // Arrange
        var venue = CreateTestVenue(40.4093, 49.8671, 150);
        var cmd = new CheckInCommand("valid-qr", 40.4094, 49.8672, "device-123") { UserId = Guid.NewGuid() };

        _venueRepo.GetByQrPayloadAsync("valid-qr", Arg.Any<CancellationToken>()).Returns(venue);
        _checkInRepo.GetRecentAsync(cmd.UserId, venue.Id, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns((CheckIn?)null);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.VenueId.Should().Be(venue.Id);
        result.Value.VenueName.Should().Be(venue.Name);

        await _checkInRepo.Received(1).AddAsync(Arg.Any<CheckIn>(), Arg.Any<CancellationToken>());
        await _redis.Received(1).IncrementVenueCountAsync(venue.Id);
        await _redis.Received(1).AddActiveUserAsync(venue.Id, cmd.UserId);
    }

    [Fact]
    public async Task Should_ReturnError_When_QrCodeIsInvalid()
    {
        var cmd = new CheckInCommand("invalid-qr", 40.4094, 49.8672, "device-123") { UserId = Guid.NewGuid() };
        _venueRepo.GetByQrPayloadAsync("invalid-qr", Arg.Any<CancellationToken>()).Returns((Venue?)null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("INVALID_QR");
    }

    [Fact]
    public async Task Should_ReturnError_When_OutsideGeofence()
    {
        var venue = CreateTestVenue(40.4093, 49.8671, 150);
        var cmd = new CheckInCommand("valid-qr", 41.0, 50.0, "device-123") { UserId = Guid.NewGuid() };
        _venueRepo.GetByQrPayloadAsync("valid-qr", Arg.Any<CancellationToken>()).Returns(venue);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("OUTSIDE_GEOFENCE");
    }

    [Fact]
    public async Task Should_ReturnError_When_RateLimited()
    {
        var venue = CreateTestVenue(40.4093, 49.8671, 150);
        var userId = Guid.NewGuid();
        var cmd = new CheckInCommand("valid-qr", 40.4094, 49.8672, "device-123") { UserId = userId };

        _venueRepo.GetByQrPayloadAsync("valid-qr", Arg.Any<CancellationToken>()).Returns(venue);
        _checkInRepo.GetRecentAsync(userId, venue.Id, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(new CheckIn { UserId = userId, VenueId = venue.Id });

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("RATE_LIMITED");
    }

    private static Venue CreateTestVenue(double lat, double lng, int radius)
    {
        return new Venue
        {
            Id = Guid.NewGuid(),
            Name = "Test Venue",
            Address = "Test Address",
            Category = VenueCategory.Bar,
            Location = new Point(lng, lat) { SRID = 4326 },
            Latitude = lat,
            Longitude = lng,
            GeofenceRadiusMeters = radius
        };
    }
}
