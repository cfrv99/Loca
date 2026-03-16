using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Commands;

public record UpdateProfileCommand(
    string? DisplayName,
    string? Bio,
    List<string>? Interests,
    List<string>? Purposes,
    List<VibePreferenceDto>? VibePreferences
) : IRequest<Result<UserDto>>
{
    public Guid UserId { get; init; }
}
