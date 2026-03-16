using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public VenueCategory Category { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int GeofenceRadiusMeters { get; set; } = 150;
    public string? CoverPhotoUrl { get; set; }
    public List<string> PhotoUrls { get; set; } = new();
    public decimal? GoogleRating { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? WorkingHoursJson { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? OwnerUserId { get; set; }
    public string SubscriptionPlan { get; set; } = "basic";
    public string QrSecretKey { get; set; } = Guid.NewGuid().ToString("N");

    // Navigation
    public List<CheckIn> CheckIns { get; set; } = new();

    public bool IsWithinGeofence(double lat, double lng)
    {
        const double earthRadiusMeters = 6371000;
        var dLat = ToRadians(lat - Latitude);
        var dLng = ToRadians(lng - Longitude);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(lat)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = earthRadiusMeters * c;
        return distance <= GeofenceRadiusMeters;
    }

    public static double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusMeters = 6371000;
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
