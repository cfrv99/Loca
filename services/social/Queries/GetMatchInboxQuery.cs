using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Loca.Services.Social.Queries;

public record GetMatchInboxQuery(
    [FromQuery] string? Status = null,
    [FromQuery] string? Cursor = null,
    [FromQuery] int PageSize = 20
) : IRequest<Result<CursorPageResponse<MatchRequestDto>>>
{
    public Guid UserId { get; init; }
}
