using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class GetVenueDetailHandler : IRequestHandler<GetVenueDetailQuery, Result<VenueDetailDto>>
{
    private readonly IVenueRepository _venues;
    private readonly IRedisService _redis;
    private readonly ILogger<GetVenueDetailHandler> _logger;

    public GetVenueDetailHandler(IVenueRepository venues, IRedisService redis, ILogger<GetVenueDetailHandler> logger)
    {
        _venues = venues;
        _redis = redis;
        _logger = logger;
    }

    public async Task<Result<VenueDetailDto>> Handle(GetVenueDetailQuery query, CancellationToken ct)
    {
        var venue = await _venues.GetByIdAsync(query.VenueId, ct);
        if (venue is null)
            return Result<VenueDetailDto>.Failure("VENUE_NOT_FOUND", "Məkan tapılmadı");

        var stats = await _redis.GetVenueStatsAsync(venue.Id);

        return Result<VenueDetailDto>.Success(new VenueDetailDto(
            Id: venue.Id,
            Name: venue.Name,
            Description: venue.Description,
            Address: venue.Address,
            Category: venue.Category.ToString(),
            Latitude: venue.Latitude,
            Longitude: venue.Longitude,
            CoverPhotoUrl: venue.CoverPhotoUrl,
            PhotoUrls: venue.PhotoUrls,
            GoogleRating: venue.GoogleRating,
            Phone: venue.Phone,
            Website: venue.Website,
            WorkingHours: venue.WorkingHoursJson,
            Stats: stats,
            GeofenceRadius: venue.GeofenceRadiusMeters
        ));
    }
}
