using FluentAssertions;
using Loca.Domain.Entities;
using Loca.Domain.Enums;

namespace Loca.Tests.Unit.Domain;

public class VenueTests
{
    [Fact]
    public void Should_BeWithinGeofence_When_InsideRadius()
    {
        var venue = new Venue
        {
            Latitude = 40.4093,
            Longitude = 49.8671,
            GeofenceRadiusMeters = 150
        };

        // Point very close to venue (~10m away)
        var result = venue.IsWithinGeofence(40.4094, 49.8672);
        result.Should().BeTrue();
    }

    [Fact]
    public void Should_NotBeWithinGeofence_When_OutsideRadius()
    {
        var venue = new Venue
        {
            Latitude = 40.4093,
            Longitude = 49.8671,
            GeofenceRadiusMeters = 150
        };

        // Point ~5km away
        var result = venue.IsWithinGeofence(40.45, 49.90);
        result.Should().BeFalse();
    }

    [Fact]
    public void Should_CalculateDistance_Correctly()
    {
        // Baku city center coordinates
        var distance = Venue.CalculateDistance(40.4093, 49.8671, 40.4100, 49.8680);
        distance.Should().BeGreaterThan(0);
        distance.Should().BeLessThan(200); // Should be under 200m for these close points
    }

    [Fact]
    public void Should_ValidateQrCode_CurrentWindow()
    {
        var secretKey = "test-secret-key-12345";
        var code = QrCodeGenerator.GenerateTotp(secretKey);
        var isValid = QrCodeGenerator.ValidateTotp(secretKey, code);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Should_RejectInvalidQrCode()
    {
        var secretKey = "test-secret-key-12345";
        var isValid = QrCodeGenerator.ValidateTotp(secretKey, "000000");
        isValid.Should().BeFalse();
    }
}
