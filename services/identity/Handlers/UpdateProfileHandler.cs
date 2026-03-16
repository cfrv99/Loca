using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Identity.Handlers;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private readonly IUserRepository _users;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(IUserRepository users, ILogger<UpdateProfileHandler> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct);
        if (user is null)
            return Result<UserDto>.Failure("USER_NOT_FOUND", "User not found");

        if (cmd.DisplayName is not null) user.DisplayName = cmd.DisplayName;
        if (cmd.Bio is not null) user.Bio = cmd.Bio;
        if (cmd.Interests is not null) user.Interests = cmd.Interests;
        if (cmd.Purposes is not null) user.Purposes = cmd.Purposes;

        await _users.UpdateAsync(user, ct);
        _logger.LogInformation("Profile updated for user {UserId}", cmd.UserId);

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
