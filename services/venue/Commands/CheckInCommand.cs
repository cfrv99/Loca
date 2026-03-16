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
    public string Gender { get; init; } = "unknown";
}
