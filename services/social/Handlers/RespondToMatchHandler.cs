using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Social.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Social.Handlers;

public class RespondToMatchHandler : IRequestHandler<RespondToMatchCommand, Result<RespondToMatchResult>>
{
    private readonly IMatchRepository _matches;
    private readonly ILogger<RespondToMatchHandler> _logger;

    public RespondToMatchHandler(IMatchRepository matches, ILogger<RespondToMatchHandler> logger)
    {
        _matches = matches;
        _logger = logger;
    }

    public async Task<Result<RespondToMatchResult>> Handle(RespondToMatchCommand cmd, CancellationToken ct)
    {
        var request = await _matches.GetByIdAsync(cmd.MatchId, ct);
        if (request is null)
            return Result<RespondToMatchResult>.Failure("NOT_FOUND", "Sorğu tapılmadı");

        if (request.ReceiverId != cmd.UserId)
            return Result<RespondToMatchResult>.Failure("FORBIDDEN", "Bu sorğuya cavab vermək icazəsi yoxdur");

        if (request.Status != MatchRequestStatus.Pending)
            return Result<RespondToMatchResult>.Failure("ALREADY_RESPONDED", "Sorğuya artıq cavab verilib");

        if (request.ExpiresAt < DateTime.UtcNow)
            return Result<RespondToMatchResult>.Failure("EXPIRED", "Sorğunun vaxtı keçib");

        request.RespondedAt = DateTime.UtcNow;

        if (cmd.Action.ToLowerInvariant() == "accept")
        {
            request.Status = MatchRequestStatus.Accepted;
            await _matches.UpdateAsync(request, ct);

            // Create conversation
            var conversation = new Conversation
            {
                MatchRequestId = request.Id,
                Participant1Id = request.SenderId,
                Participant2Id = request.ReceiverId
            };
            await _matches.AddConversationAsync(conversation, ct);

            _logger.LogInformation("Match {MatchId} accepted, conversation {ConversationId} created",
                request.Id, conversation.Id);

            return Result<RespondToMatchResult>.Success(new RespondToMatchResult(conversation.Id));
        }
        else
        {
            request.Status = MatchRequestStatus.Declined;
            await _matches.UpdateAsync(request, ct);

            _logger.LogInformation("Match {MatchId} declined", request.Id);

            return Result<RespondToMatchResult>.Success(new RespondToMatchResult(null));
        }
    }
}
