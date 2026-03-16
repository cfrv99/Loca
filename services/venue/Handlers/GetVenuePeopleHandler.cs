using Loca.Application.Common;
using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Venue.Handlers;

public class GetVenuePeopleHandler : IRequestHandler<GetVenuePeopleQuery, Result<CursorPageResponse<ActiveUserDto>>>
{
    private readonly ICheckInRepository _checkIns;
    private readonly IRedisService _redis;
    private readonly ILogger<GetVenuePeopleHandler> _logger;

    public GetVenuePeopleHandler(ICheckInRepository checkIns, IRedisService redis, ILogger<GetVenuePeopleHandler> logger)
    {
        _checkIns = checkIns;
        _redis = redis;
        _logger = logger;
    }

    public async Task<Result<CursorPageResponse<ActiveUserDto>>> Handle(GetVenuePeopleQuery query, CancellationToken ct)
    {
        var activeCheckIns = await _checkIns.GetActiveCheckInsForVenueAsync(query.VenueId, ct);

        // Filter out anonymous users from the visible list
        var visibleCheckIns = activeCheckIns
            .Where(c => !c.IsAnonymous && c.User is not null)
            .OrderByDescending(c => c.CheckInAt)
            .ToList();

        // Cursor pagination
        if (!string.IsNullOrEmpty(query.Cursor) && Guid.TryParse(query.Cursor, out var cursorId))
        {
            var cursorIndex = visibleCheckIns.FindIndex(c => c.Id == cursorId);
            if (cursorIndex >= 0)
                visibleCheckIns = visibleCheckIns.Skip(cursorIndex + 1).ToList();
        }

        var hasMore = visibleCheckIns.Count > query.PageSize;
        var items = visibleCheckIns.Take(query.PageSize).ToList();

        var users = items.Select(c => new ActiveUserDto(
            UserId: c.UserId,
            DisplayName: c.User!.DisplayName,
            AvatarUrl: c.User.AvatarUrl,
            Age: c.User.GetAge(),
            Interests: c.User.Interests,
            IsAnonymous: false
        )).ToList();

        return Result<CursorPageResponse<ActiveUserDto>>.Success(new CursorPageResponse<ActiveUserDto>
        {
            Items = users,
            NextCursor = hasMore && items.Count > 0 ? items.Last().Id.ToString() : null,
            HasMore = hasMore
        });
    }
}
