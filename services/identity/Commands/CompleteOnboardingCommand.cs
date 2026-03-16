using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Commands;

public record CompleteOnboardingCommand(
    List<string> Interests,
    List<string> Purposes,
    List<VibePreferenceDto> VibePreferences,
    PrivacySettingsDto PrivacySettings
) : IRequest<Result<UserDto>>
{
    public Guid UserId { get; init; }
}
