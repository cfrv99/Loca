using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Venue.Commands;

public record CheckInCommand(
    string QrPayload,
    double Lat,
    double Lng,
    string DeviceFingerprint,
    bool IsAnonymous = false
) : IRequest<Result<CheckInResultDto>>
{
    public Guid UserId { get; init; }
}

public record CheckOutCommand(
    Guid VenueId
) : IRequest<Result<bool>>
{
    public Guid UserId { get; init; }
}
