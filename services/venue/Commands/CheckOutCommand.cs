using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Venue.Commands;

public record CheckOutCommand(Guid CheckInId) : IRequest<Result<CheckOutResultDto>>
{
    public Guid UserId { get; init; }
}

public record CheckOutResultDto(bool Success, string Duration);
