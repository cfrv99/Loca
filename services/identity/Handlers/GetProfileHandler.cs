using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Queries;
using MediatR;

namespace Loca.Services.Identity.Handlers;

public class GetMyProfileHandler : IRequestHandler<GetMyProfileQuery, Result<UserDto>>
{
    private readonly IUserRepository _users;

    public GetMyProfileHandler(IUserRepository users) => _users = users;

    public async Task<Result<UserDto>> Handle(GetMyProfileQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return Result<UserDto>.Failure("USER_NOT_FOUND", "User not found");

        return Result<UserDto>.Success(new UserDto(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            FirstName: user.FirstName,
            LastName: user.LastName,
            ProfilePhotoUrl: user.ProfilePhotoUrl,
            Bio: user.Bio,
            IsOnboardingComplete: user.IsOnboardingComplete,
            CreatedAt: user.CreatedAt
        ));
    }
}

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IUserRepository _users;

    public GetUserProfileHandler(IUserRepository users) => _users = users;

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.TargetUserId, ct);
        if (user is null)
            return Result<UserProfileDto>.Failure("USER_NOT_FOUND", "User not found");

        return Result<UserProfileDto>.Success(new UserProfileDto(
            Id: user.Id,
            DisplayName: user.DisplayName,
            ProfilePhotoUrl: user.ProfilePhotoUrl,
            Bio: user.Bio,
            Interests: user.Interests,
            TotalCheckIns: user.Profile?.TotalCheckIns ?? 0,
            TotalGamesPlayed: user.Profile?.TotalGamesPlayed ?? 0,
            TotalGiftsReceived: user.Profile?.TotalGiftsReceived ?? 0,
            TotalMatchesMade: user.Profile?.TotalMatchesMade ?? 0
        ));
    }
}
