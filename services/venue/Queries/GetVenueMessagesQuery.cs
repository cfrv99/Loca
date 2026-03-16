using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Loca.Services.Venue.Queries;

public record GetVenueMessagesQuery(
    Guid VenueId,
    [FromQuery] string? Cursor = null,
    [FromQuery] int PageSize = 50
) : IRequest<Result<CursorPageResponse<ChatMessageDto>>>;
