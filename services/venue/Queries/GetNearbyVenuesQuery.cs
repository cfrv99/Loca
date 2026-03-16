using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Loca.Services.Venue.Queries;

public record GetNearbyVenuesQuery(
    [FromQuery] double Lat,
    [FromQuery] double Lng,
    [FromQuery] int Radius = 5000,
    [FromQuery] string? Category = null,
    [FromQuery] string? Cursor = null,
    [FromQuery] int PageSize = 20
) : IRequest<Result<CursorPageResponse<VenueCardDto>>>;
