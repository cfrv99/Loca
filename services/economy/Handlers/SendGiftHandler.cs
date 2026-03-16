using Loca.Application.DTOs;
using Loca.Domain.Common;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Economy.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Economy.Handlers;

public class SendGiftHandler : IRequestHandler<SendGiftCommand, Result<SendGiftResponse>>
{
    private readonly IEconomyRepository _economy;
    private readonly ILogger<SendGiftHandler> _logger;

    public SendGiftHandler(IEconomyRepository economy, ILogger<SendGiftHandler> logger)
    {
        _economy = economy;
        _logger = logger;
    }

    public async Task<Result<SendGiftResponse>> Handle(SendGiftCommand cmd, CancellationToken ct)
    {
        // 1. Get gift info
        var gift = await _economy.GetGiftByIdAsync(cmd.GiftId, ct);
        if (gift is null)
            return Result<SendGiftResponse>.Failure("GIFT_NOT_FOUND", "Hədiyyə tapılmadı");

        // 2. Get wallet and check balance
        var wallet = await _economy.GetOrCreateWalletAsync(cmd.UserId, ct);
        if (!wallet.CanAfford(gift.CoinPrice))
            return Result<SendGiftResponse>.Failure("INSUFFICIENT_BALANCE", "Kifayət qədər coin yoxdur");

        // 3. Debit wallet
        wallet.Debit(gift.CoinPrice);
        await _economy.UpdateWalletAsync(wallet, ct);

        // 4. Create transaction
        var transaction = new Transaction
        {
            UserId = cmd.UserId,
            Type = TransactionType.GiftSent,
            Amount = -gift.CoinPrice,
            BalanceAfter = wallet.CoinBalance,
            ReferenceType = "gift",
            ReferenceId = gift.Id,
            Description = $"Hədiyyə göndərildi: {gift.NameAz ?? gift.Name}"
        };
        await _economy.AddTransactionAsync(transaction, ct);

        _logger.LogInformation("User {UserId} sent gift {GiftId} ({GiftName}) to {RecipientId}, cost: {Cost} coins",
            cmd.UserId, gift.Id, gift.Name, cmd.RecipientId, gift.CoinPrice);

        return Result<SendGiftResponse>.Success(new SendGiftResponse(
            TransactionId: transaction.Id,
            NewBalance: wallet.CoinBalance
        ));
    }
}
