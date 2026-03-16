using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Queries;

public record GetUserProfileQuery(Guid UserId, Guid RequesterId) : IRequest<Result<PublicUserDto>>;
