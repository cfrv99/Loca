using FluentAssertions;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using NetTopologySuite.Geometries;

namespace Loca.Tests.Unit.Domain;

public class VenueTests
{
    [Fact]
    public void Should_ReturnTrue_When_UserIsWithinGeofence()
    {
        // Arrange — Sea Breeze Beach Club, Baku
        var venue = CreateTestVenue(40.5530, 50.3580, 150);

        // User is very close (about 50m)
        var isWithin = venue.IsWithinGeofence(40.5534, 50.3583);

        isWithin.Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_UserIsOutsideGeofence()
    {
        var venue = CreateTestVenue(40.5530, 50.3580, 150);

        // User is far away (different city)
        var isWithin = venue.IsWithinGeofence(41.0, 49.0);

        isWithin.Should().BeFalse();
    }

    [Fact]
    public void Should_UseDefaultGeofenceRadius()
    {
        var venue = new Venue
        {
            Name = "Test Venue",
            Location = new Point(49.8671, 40.4093) { SRID = 4326 }
        };

        venue.GeofenceRadiusMeters.Should().Be(150);
    }

    private static Venue CreateTestVenue(double lat, double lng, int radius)
    {
        return new Venue
        {
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
