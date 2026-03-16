using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Identity.Handlers;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, Result<PublicUserDto>>
{
    private readonly IUserRepository _users;
    private readonly IMatchRepository _matches;
    private readonly ILogger<GetUserProfileHandler> _logger;

    public GetUserProfileHandler(IUserRepository users, IMatchRepository matches, ILogger<GetUserProfileHandler> logger)
    {
        _users = users;
        _matches = matches;
        _logger = logger;
    }

    public async Task<Result<PublicUserDto>> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        // Check if blocked
        var isBlocked = await _matches.IsBlockedAsync(query.RequesterId, query.UserId, ct);
        if (isBlocked)
            return Result<PublicUserDto>.Failure("USER_BLOCKED", "Bu istifadəçi bloklanıb");

        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null || !user.IsActive || user.IsDeleted)
            return Result<PublicUserDto>.Failure("USER_NOT_FOUND", "İstifadəçi tapılmadı");

        return Result<PublicUserDto>.Success(new PublicUserDto(
            Id: user.Id,
            DisplayName: user.DisplayName,
            AvatarUrl: user.AvatarUrl,
            Age: user.GetAge(),
            Gender: user.Gender.ToString(),
            Interests: user.Interests,
            Purposes: user.Purposes,
            PastVenues: new List<PastVenueDto>(), // TODO: populate from check-in history
            MemoriesCount: 0, // TODO: count posts
            GiftBadge: null // TODO: compute from gift leaderboard
        ));
    }
}
