using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class GetNearbyVenuesHandler : IRequestHandler<GetNearbyVenuesQuery, Result<CursorPageResponse<VenueCardDto>>>
{
    private readonly IVenueRepository _venues;
    private readonly IRedisService _redis;
    private readonly ILogger<GetNearbyVenuesHandler> _logger;

    public GetNearbyVenuesHandler(IVenueRepository venues, IRedisService redis, ILogger<GetNearbyVenuesHandler> logger)
    {
        _venues = venues;
        _redis = redis;
        _logger = logger;
    }

    public async Task<Result<CursorPageResponse<VenueCardDto>>> Handle(GetNearbyVenuesQuery query, CancellationToken ct)
    {
        var venues = await _venues.GetNearbyAsync(
            query.Lat, query.Lng, query.Radius, query.Category, query.PageSize + 1, query.Cursor, ct);

        var hasMore = venues.Count > query.PageSize;
        var items = venues.Take(query.PageSize).ToList();

        var venueCards = new List<VenueCardDto>();
        foreach (var venue in items)
        {
            var stats = await _redis.GetVenueStatsAsync(venue.Id);
            var distance = Domain.Entities.Venue.CalculateDistance(query.Lat, query.Lng, venue.Latitude, venue.Longitude);
            var activityLevel = stats.Total > 20 ? "high" : stats.Total > 5 ? "medium" : "low";

            venueCards.Add(new VenueCardDto(
                Id: venue.Id,
                Name: venue.Name,
                CoverPhotoUrl: venue.CoverPhotoUrl,
                Address: venue.Address,
                Category: venue.Category.ToString(),
                DistanceMeters: Math.Round(distance, 0),
                Stats: stats,
                ActivityLevel: activityLevel,
                ActiveGames: 0,
                ChatMessageCount: 0
            ));
        }

        return Result<CursorPageResponse<VenueCardDto>>.Success(new CursorPageResponse<VenueCardDto>
        {
            Items = venueCards,
            NextCursor = hasMore ? items.Last().Id.ToString() : null,
            HasMore = hasMore
        });
    }
}
