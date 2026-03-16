using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Identity.Queries;

public record GetMyProfileQuery(Guid UserId) : IRequest<Result<UserDto>>;

public record GetUserProfileQuery(Guid UserId, Guid TargetUserId) : IRequest<Result<UserProfileDto>>;
