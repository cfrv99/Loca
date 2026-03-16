using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Enums;
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

        // Update fields if provided
        if (cmd.DisplayName is not null) user.DisplayName = cmd.DisplayName;
        if (cmd.FirstName is not null) user.FirstName = cmd.FirstName;
        if (cmd.LastName is not null) user.LastName = cmd.LastName;
        if (cmd.Bio is not null) user.Bio = cmd.Bio;
        if (cmd.DateOfBirth is not null) user.DateOfBirth = cmd.DateOfBirth;
        if (cmd.Gender is not null && Enum.TryParse<Gender>(cmd.Gender, out var gender)) user.Gender = gender;
        if (cmd.Interests is not null) user.Interests = cmd.Interests;
        if (cmd.Purposes is not null)
            user.Purposes = cmd.Purposes
                .Select(p => Enum.TryParse<UserPurpose>(p, out var purpose) ? purpose : (UserPurpose?)null)
                .Where(p => p.HasValue)
                .Select(p => p!.Value)
                .ToList();
        if (cmd.VibePreference is not null && Enum.TryParse<VibePreference>(cmd.VibePreference, out var vibe))
            user.VibePreference = vibe;
        if (cmd.PrivacyLevel is not null && Enum.TryParse<PrivacyLevel>(cmd.PrivacyLevel, out var privacy))
            user.PrivacyLevel = privacy;
        if (cmd.CompleteOnboarding == true)
            user.IsOnboardingComplete = true;

        await _users.UpdateAsync(user, ct);

        _logger.LogInformation("Profile updated for user {UserId}", cmd.UserId);

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
