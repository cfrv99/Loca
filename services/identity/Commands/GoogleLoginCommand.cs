using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Commands;

public record GoogleLoginCommand(
    string IdToken,
    string? DeviceInfo
) : IRequest<Result<LoginResultDto>>;

public record AppleLoginCommand(
    string IdToken,
    string? DeviceInfo
) : IRequest<Result<LoginResultDto>>;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<RefreshTokenDto>>;
