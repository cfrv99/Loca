using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Queries;

public record GetProfileQuery(Guid UserId) : IRequest<Result<UserDto>>;
