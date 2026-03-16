using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Social.Commands;

public record RespondToMatchCommand(
    Guid MatchId,
    string Action // "accept" or "decline"
) : IRequest<Result<RespondToMatchResult>>
{
    public Guid UserId { get; init; }
}

public record RespondToMatchResult(Guid? ConversationId);
