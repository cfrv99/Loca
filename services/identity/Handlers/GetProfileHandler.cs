using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Queries;
using MediatR;

namespace Loca.Services.Identity.Handlers;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, Result<UserDto>>
{
    private readonly IUserRepository _users;

    public GetProfileHandler(IUserRepository users) => _users = users;

    public async Task<Result<UserDto>> Handle(GetProfileQuery query, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(query.UserId, ct);
        if (user is null)
            return Result<UserDto>.Failure("USER_NOT_FOUND", "User not found");

        return Result<UserDto>.Success(new UserDto(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            AvatarUrl: user.AvatarUrl,
            DateOfBirth: user.DateOfBirth,
            Gender: user.Gender.ToString(),
            Interests: user.Interests,
            Purposes: user.Purposes,
            VibePreferences: user.VibePreferences.Select(v => new VibePreferenceDto(v.Vibe, v.Weight)).ToList(),
            IsOnboarded: user.IsOnboarded,
            IsPremium: user.IsPremium,
            CoinBalance: 0,
            CreatedAt: user.CreatedAt
        ));
    }
}
