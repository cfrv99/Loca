using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Identity.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Identity.Handlers;

public class CompleteOnboardingHandler : IRequestHandler<CompleteOnboardingCommand, Result<UserDto>>
{
    private readonly IUserRepository _users;
    private readonly ILogger<CompleteOnboardingHandler> _logger;

    public CompleteOnboardingHandler(IUserRepository users, ILogger<CompleteOnboardingHandler> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(CompleteOnboardingCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(cmd.UserId, ct);
        if (user is null)
            return Result<UserDto>.Failure("USER_NOT_FOUND", "İstifadəçi tapılmadı");

        if (user.IsOnboarded)
            return Result<UserDto>.Failure("ALREADY_ONBOARDED", "Onboarding artıq tamamlanıb");

        // Update interests, purposes, vibe preferences
        user.Interests = cmd.Interests;
        user.Purposes = cmd.Purposes;
        user.VibePreferences = cmd.VibePreferences.Select(v => new UserVibePreference
        {
            UserId = user.Id,
            Vibe = v.Vibe,
            Weight = v.Weight
        }).ToList();
        user.IsAnonymousDefault = cmd.PrivacySettings.DefaultAnonymous;
        user.IsOnboarded = true;

        await _users.UpdateAsync(user, ct);
        _logger.LogInformation("User {UserId} completed onboarding", cmd.UserId);

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
