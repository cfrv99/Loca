using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Social.Commands;

public record ReportUserCommand(
    Guid ReportedId,
    string Reason,
    string? Description
) : IRequest<Result<ReportUserResult>>
{
    public Guid ReporterId { get; init; }
}

public record ReportUserResult(Guid ReportId);
