using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Interfaces;
using Loca.Services.Economy.Queries;
using MediatR;

namespace Loca.Services.Economy.Handlers;

public class GetBalanceHandler : IRequestHandler<GetBalanceQuery, Result<BalanceDto>>
{
    private readonly IEconomyRepository _economy;

    public GetBalanceHandler(IEconomyRepository economy) => _economy = economy;

    public async Task<Result<BalanceDto>> Handle(GetBalanceQuery query, CancellationToken ct)
    {
        var wallet = await _economy.GetOrCreateWalletAsync(query.UserId, ct);

        return Result<BalanceDto>.Success(new BalanceDto(
            CoinBalance: wallet.CoinBalance,
            TotalPurchased: wallet.TotalPurchased,
            TotalSpent: wallet.TotalSpent
        ));
    }
}
