using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Social.Commands;

public record SendMatchRequestCommand(
    Guid ReceiverId,
    string? IntroMessage
) : IRequest<Result<MatchRequestDto>>
{
    public Guid UserId { get; init; }
}
