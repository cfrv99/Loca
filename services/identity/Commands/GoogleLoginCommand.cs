using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Commands;

public record GoogleLoginCommand(string IdToken) : IRequest<Result<AuthResponse>>;
