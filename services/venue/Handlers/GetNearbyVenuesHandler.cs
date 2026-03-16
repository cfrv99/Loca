using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Queries;
using MediatR;
using NetTopologySuite.Geometries;

namespace Loca.Services.Venue.Handlers;

public class GetNearbyVenuesHandler : IRequestHandler<GetNearbyVenuesQuery, Result<CursorPageResponse<VenueCardDto>>>
{
    private readonly IVenueRepository _venues;
    private readonly IRedisService _redis;

    public GetNearbyVenuesHandler(IVenueRepository venues, IRedisService redis)
    {
        _venues = venues;
        _redis = redis;
    }

    public async Task<Result<CursorPageResponse<VenueCardDto>>> Handle(GetNearbyVenuesQuery query, CancellationToken ct)
    {
        var radius = query.RadiusMeters ?? 5000; // Default 5km
        var venues = await _venues.GetNearbyAsync(query.Lat, query.Lng, radius, query.PageSize, ct);
        var userLocation = new Point(query.Lng, query.Lat) { SRID = 4326 };

        var venueDtos = new List<VenueCardDto>();
        foreach (var venue in venues)
        {
            var activeCount = await _redis.GetVenueCountAsync(venue.Id);
            var distanceMeters = venue.Location.Distance(userLocation) * 111320;

            var activityLevel = activeCount switch
            {
                >= 30 => "high",
                >= 10 => "medium",
                _ => "low"
            };

            venueDtos.Add(new VenueCardDto(
                Id: venue.Id,
                Name: venue.Name,
                Address: venue.Address,
                Category: venue.Category.ToString(),
                CoverPhotoUrl: venue.CoverPhotoUrl,
                Latitude: venue.Latitude,
                Longitude: venue.Longitude,
                DistanceMeters: distanceMeters,
                ActiveCount: activeCount,
                MaleCount: 0, // Will be calculated when gender data is available
                FemaleCount: 0,
                ActivityLevel: activityLevel
            ));
        }

        return Result<CursorPageResponse<VenueCardDto>>.Success(new CursorPageResponse<VenueCardDto>
        {
            Items = venueDtos,
            HasMore = venues.Count == query.PageSize,
            NextCursor = venues.Count == query.PageSize ? venues.Last().Id.ToString() : null,
            TotalCount = venueDtos.Count
        });
    }
}

public class GetVenueDetailHandler : IRequestHandler<GetVenueDetailQuery, Result<VenueDetailDto>>
{
    private readonly IVenueRepository _venues;
    private readonly IRedisService _redis;

    public GetVenueDetailHandler(IVenueRepository venues, IRedisService redis)
    {
        _venues = venues;
        _redis = redis;
    }

    public async Task<Result<VenueDetailDto>> Handle(GetVenueDetailQuery query, CancellationToken ct)
    {
        var venue = await _venues.GetByIdAsync(query.VenueId, ct);
        if (venue is null)
            return Result<VenueDetailDto>.Failure("VENUE_NOT_FOUND", "Venue not found");

        var activeCount = await _redis.GetVenueCountAsync(venue.Id);

        return Result<VenueDetailDto>.Success(new VenueDetailDto(
            Id: venue.Id,
            Name: venue.Name,
            Description: venue.Description,
            Address: venue.Address,
            Category: venue.Category.ToString(),
            CoverPhotoUrl: venue.CoverPhotoUrl,
            PhotoUrls: venue.PhotoUrls,
            Phone: venue.Phone,
            Website: venue.Website,
            InstagramHandle: venue.InstagramHandle,
            Latitude: venue.Latitude,
            Longitude: venue.Longitude,
            GeofenceRadiusMeters: venue.GeofenceRadiusMeters,
            OpeningHours: venue.OpeningHours,
            ActiveCount: activeCount,
            IsVerified: venue.IsVerified,
            CreatedAt: venue.CreatedAt
        ));
    }
}
