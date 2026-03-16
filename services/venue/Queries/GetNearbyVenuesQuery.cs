using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Venue.Queries;

public record GetNearbyVenuesQuery(
    double Lat,
    double Lng,
    double? RadiusMeters,
    string? Cursor,
    int PageSize = 20
) : IRequest<Result<CursorPageResponse<VenueCardDto>>>;

public record GetVenueDetailQuery(Guid VenueId) : IRequest<Result<VenueDetailDto>>;
