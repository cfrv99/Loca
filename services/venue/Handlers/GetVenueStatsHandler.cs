using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Queries;
using MediatR;

namespace Loca.Services.Venue.Handlers;

public class GetVenueStatsHandler : IRequestHandler<GetVenueStatsQuery, Result<VenueStatsDto>>
{
    private readonly IVenueRepository _venues;
    private readonly IRedisService _redis;

    public GetVenueStatsHandler(IVenueRepository venues, IRedisService redis)
    {
        _venues = venues;
        _redis = redis;
    }

    public async Task<Result<VenueStatsDto>> Handle(GetVenueStatsQuery query, CancellationToken ct)
    {
        var venue = await _venues.GetByIdAsync(query.VenueId, ct);
        if (venue is null)
            return Result<VenueStatsDto>.Failure("VENUE_NOT_FOUND", "Məkan tapılmadı");

        var stats = await _redis.GetVenueStatsAsync(venue.Id);
        return Result<VenueStatsDto>.Success(stats);
    }
}
