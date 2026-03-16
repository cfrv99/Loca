using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Loca.Services.Venue.Queries;

public record GetCommentsQuery(
    Guid PostId,
    [FromQuery] string? Cursor = null,
    [FromQuery] int PageSize = 20
) : IRequest<Result<CursorPageResponse<CommentDto>>>;
