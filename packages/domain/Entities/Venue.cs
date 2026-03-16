using Loca.Domain.Common;
using Loca.Domain.Enums;
using NetTopologySuite.Geometries;

namespace Loca.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public VenueCategory Category { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? InstagramHandle { get; set; }

    // Location (PostGIS)
    public Point Location { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int GeofenceRadiusMeters { get; set; } = 150;

    // Operating
    public string? OpeningHours { get; set; } // JSON string
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }

    // Navigation
    public List<QrCode> QrCodes { get; set; } = new();
    public List<CheckIn> CheckIns { get; set; } = new();

    public bool IsWithinGeofence(double lat, double lng)
    {
        var userLocation = new Point(lng, lat) { SRID = 4326 };
        return Location.Distance(userLocation) * 111320 <= GeofenceRadiusMeters;
    }
}
