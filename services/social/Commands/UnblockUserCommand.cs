using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Social.Commands;

public record UnblockUserCommand(Guid BlockedId) : IRequest<Result<BlockResponse>>
{
    public Guid BlockerId { get; init; }
}
