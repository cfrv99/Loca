using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Venue.Queries;

public record GetVenueStatsQuery(Guid VenueId) : IRequest<Result<VenueStatsDto>>;
