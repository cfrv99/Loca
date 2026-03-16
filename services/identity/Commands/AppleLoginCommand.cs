using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Commands;

public record AppleLoginCommand(
    string IdentityToken,
    string AuthorizationCode,
    string? FullName
) : IRequest<Result<AuthResponse>>;
