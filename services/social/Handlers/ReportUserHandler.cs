using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Social.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Social.Handlers;

public class ReportUserHandler : IRequestHandler<ReportUserCommand, Result<ReportUserResult>>
{
    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;
    private readonly ILogger<ReportUserHandler> _logger;

    public ReportUserHandler(IMatchRepository matches, IUserRepository users, ILogger<ReportUserHandler> logger)
    {
        _matches = matches;
        _users = users;
        _logger = logger;
    }

    public async Task<Result<ReportUserResult>> Handle(ReportUserCommand cmd, CancellationToken ct)
    {
        if (cmd.ReporterId == cmd.ReportedId)
            return Result<ReportUserResult>.Failure("INVALID_REQUEST", "Özünüzü şikayət edə bilməzsiniz");

        var reportedUser = await _users.GetByIdAsync(cmd.ReportedId, ct);
        if (reportedUser is null)
            return Result<ReportUserResult>.Failure("USER_NOT_FOUND", "İstifadəçi tapılmadı");

        if (!Enum.TryParse<ReportReason>(cmd.Reason, true, out var reason))
            return Result<ReportUserResult>.Failure("INVALID_REASON", "Yanlış şikayət səbəbi");

        var report = new Report
        {
            ReporterId = cmd.ReporterId,
            ReportedId = cmd.ReportedId,
            Reason = reason,
            Description = cmd.Description
        };

        await _matches.AddReportAsync(report, ct);
        _logger.LogInformation("User {ReporterId} reported user {ReportedId} for {Reason}",
            cmd.ReporterId, cmd.ReportedId, cmd.Reason);

        return Result<ReportUserResult>.Success(new ReportUserResult(report.Id));
    }
}
