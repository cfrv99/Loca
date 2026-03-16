using Loca.Application.DTOs;
using Loca.Domain.Common;
using MediatR;

namespace Loca.Services.Economy.Queries;

public record GetBalanceQuery(Guid UserId) : IRequest<Result<BalanceDto>>;
