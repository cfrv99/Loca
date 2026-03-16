using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Venue.Commands;

public record ToggleLikeCommand(Guid PostId) : IRequest<Result<ToggleLikeResponse>>
{
    public Guid UserId { get; init; }
}
