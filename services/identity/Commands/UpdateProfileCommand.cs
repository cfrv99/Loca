using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Commands;

public record UpdateProfileCommand(
    string? DisplayName,
    string? FirstName,
    string? LastName,
    string? Bio,
    DateTime? DateOfBirth,
    string? Gender,
    List<string>? Interests,
    List<string>? Purposes,
    string? VibePreference,
    string? PrivacyLevel,
    bool? CompleteOnboarding
) : IRequest<Result<UserDto>>
{
    public Guid UserId { get; init; }
}
