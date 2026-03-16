using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Economy.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Economy.Handlers;

public class PurchaseCoinsHandler : IRequestHandler<PurchaseCoinsCommand, Result<PurchaseResponse>>
{
    private readonly IEconomyRepository _economy;
    private readonly ILogger<PurchaseCoinsHandler> _logger;

    public PurchaseCoinsHandler(IEconomyRepository economy, ILogger<PurchaseCoinsHandler> logger)
    {
        _economy = economy;
        _logger = logger;
    }

    public async Task<Result<PurchaseResponse>> Handle(PurchaseCoinsCommand cmd, CancellationToken ct)
    {
        // 1. Find coin package
        var package = await _economy.GetCoinPackageByProductIdAsync(cmd.ProductId, ct);
        if (package is null)
            return Result<PurchaseResponse>.Failure("INVALID_PRODUCT", "Məhsul tapılmadı");

        // 2. Validate receipt (placeholder - in production, validate with App Store / Google Play APIs)
        if (string.IsNullOrWhiteSpace(cmd.ReceiptData))
            return Result<PurchaseResponse>.Failure("INVALID_RECEIPT", "Qəbz etibarsızdır");

        // 3. Credit wallet
        var totalCoins = package.Coins + package.BonusCoins;
        var wallet = await _economy.GetOrCreateWalletAsync(cmd.UserId, ct);
        wallet.Credit(totalCoins);
        await _economy.UpdateWalletAsync(wallet, ct);

        // 4. Create transaction
        var transaction = new Transaction
        {
            UserId = cmd.UserId,
            Type = TransactionType.Purchase,
            Amount = totalCoins,
            BalanceAfter = wallet.CoinBalance,
            ReferenceType = "iap",
            Description = $"Coin alışı: {package.NameAz ?? package.Name} ({totalCoins} coin)"
        };
        await _economy.AddTransactionAsync(transaction, ct);

        _logger.LogInformation("User {UserId} purchased {Coins} coins (product: {ProductId})",
            cmd.UserId, totalCoins, cmd.ProductId);

        return Result<PurchaseResponse>.Success(new PurchaseResponse(
            CoinsAdded: totalCoins,
            NewBalance: wallet.CoinBalance,
            TransactionId: transaction.Id
        ));
    }
}
