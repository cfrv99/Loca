using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Social.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Social.Handlers;

public class SendMatchRequestHandler : IRequestHandler<SendMatchRequestCommand, Result<MatchRequestDto>>
{
    private readonly IMatchRepository _matches;
    private readonly ICheckInRepository _checkIns;
    private readonly IUserRepository _users;
    private readonly ILogger<SendMatchRequestHandler> _logger;

    public SendMatchRequestHandler(
        IMatchRepository matches,
        ICheckInRepository checkIns,
        IUserRepository users,
        ILogger<SendMatchRequestHandler> logger)
    {
        _matches = matches;
        _checkIns = checkIns;
        _users = users;
        _logger = logger;
    }

    public async Task<Result<MatchRequestDto>> Handle(SendMatchRequestCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == cmd.ReceiverId)
            return Result<MatchRequestDto>.Failure("INVALID_REQUEST", "Özünüzə sorğu göndərə bilməzsiniz");

        // Check block
        var isBlocked = await _matches.IsBlockedAsync(cmd.UserId, cmd.ReceiverId, ct);
        if (isBlocked)
            return Result<MatchRequestDto>.Failure("USER_BLOCKED", "Bu istifadəçi bloklanıb");

        // Check sender has active check-in
        var senderCheckIn = await _checkIns.GetActiveCheckInByUserAsync(cmd.UserId, ct);
        if (senderCheckIn is null)
            return Result<MatchRequestDto>.Failure("NOT_CHECKED_IN", "Məkana daxil olmalısınız");

        // Check sender is not anonymous
        if (senderCheckIn.IsAnonymous)
            return Result<MatchRequestDto>.Failure("ANONYMOUS_NOT_ALLOWED", "Anonim istifadəçilər sorğu göndərə bilməz");

        // Check receiver has active check-in at SAME venue
        var receiverCheckIn = await _checkIns.GetActiveCheckInAsync(cmd.ReceiverId, senderCheckIn.VenueId, ct);
        if (receiverCheckIn is null)
            return Result<MatchRequestDto>.Failure("SAME_VENUE_REQUIRED", "Hər iki istifadəçi eyni məkanda olmalıdır");

        if (receiverCheckIn.IsAnonymous)
            return Result<MatchRequestDto>.Failure("ANONYMOUS_NOT_ALLOWED", "Anonim istifadəçilərə sorğu göndərilə bilməz");

        // Check daily limit (5/day for free users)
        var dailyCount = await _matches.GetDailyRequestCountAsync(cmd.UserId, ct);
        var sender = await _users.GetByIdAsync(cmd.UserId, ct);
        var maxDailyRequests = sender?.IsPremium == true ? int.MaxValue : 5;
        if (dailyCount >= maxDailyRequests)
            return Result<MatchRequestDto>.Failure("DAILY_LIMIT_REACHED", "Günlük sorğu limiti dolub (5/gün)");

        // Check no pending request already
        var existing = await _matches.GetPendingAsync(cmd.UserId, cmd.ReceiverId, ct);
        if (existing is not null)
            return Result<MatchRequestDto>.Failure("ALREADY_PENDING", "Artıq gözləyən sorğu var");

        // Check spam protection: 3 declines = 48h block
        var declineCount = await _matches.GetDeclineCountAsync(cmd.UserId, cmd.ReceiverId, ct);
        if (declineCount >= 3)
            return Result<MatchRequestDto>.Failure("SPAM_BLOCKED", "Bu istifadəçiyə sorğu göndərə bilməzsiniz (48 saat)");

        var matchRequest = new MatchRequest
        {
            SenderId = cmd.UserId,
            ReceiverId = cmd.ReceiverId,
            VenueId = senderCheckIn.VenueId,
            IntroMessage = cmd.IntroMessage,
        };

        await _matches.AddAsync(matchRequest, ct);
        _logger.LogInformation("User {SenderId} sent match request to {ReceiverId} at venue {VenueId}",
            cmd.UserId, cmd.ReceiverId, senderCheckIn.VenueId);

        return Result<MatchRequestDto>.Success(new MatchRequestDto(
            Id: matchRequest.Id,
            SenderId: matchRequest.SenderId,
            SenderName: sender?.DisplayName ?? "Guest",
            SenderAvatar: sender?.AvatarUrl,
            IntroMessage: matchRequest.IntroMessage,
            VenueId: matchRequest.VenueId,
            Status: matchRequest.Status.ToString(),
            CreatedAt: matchRequest.CreatedAt
        ));
    }
}
