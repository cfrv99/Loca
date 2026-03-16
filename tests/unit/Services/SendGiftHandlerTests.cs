using FluentAssertions;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Economy.Commands;
using Loca.Services.Economy.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class SendGiftHandlerTests
{
    private readonly IEconomyRepository _economy = Substitute.For<IEconomyRepository>();
    private readonly ILogger<SendGiftHandler> _logger = Substitute.For<ILogger<SendGiftHandler>>();
    private readonly SendGiftHandler _handler;

    public SendGiftHandlerTests()
    {
        _handler = new SendGiftHandler(_economy, _logger);
    }

    [Fact]
    public async Task Should_ReturnError_When_GiftNotFound()
    {
        _economy.GetGiftByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((GiftCatalogItem?)null);

        var cmd = new SendGiftCommand(Guid.NewGuid(), Guid.NewGuid(), "public_chat", Guid.NewGuid(), null)
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("GIFT_NOT_FOUND");
    }

    [Fact]
    public async Task Should_ReturnError_When_InsufficientBalance()
    {
        var gift = new GiftCatalogItem { Name = "Rose", CoinPrice = 100, Tier = GiftTier.Basic };
        _economy.GetGiftByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gift);
        _economy.GetOrCreateWalletAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Wallet { CoinBalance = 50 });

        var cmd = new SendGiftCommand(gift.Id, Guid.NewGuid(), "public_chat", Guid.NewGuid(), null)
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("INSUFFICIENT_BALANCE");
    }

    [Fact]
    public async Task Should_SuccessfullySendGift_When_SufficientBalance()
    {
        var userId = Guid.NewGuid();
        var gift = new GiftCatalogItem { Name = "Rose", NameAz = "Gul", CoinPrice = 100, Tier = GiftTier.Basic };
        var wallet = new Wallet { UserId = userId, CoinBalance = 500 };

        _economy.GetGiftByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(gift);
        _economy.GetOrCreateWalletAsync(userId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var cmd = new SendGiftCommand(gift.Id, Guid.NewGuid(), "public_chat", Guid.NewGuid(), null)
        {
            UserId = userId
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.NewBalance.Should().Be(400);
        await _economy.Received(1).UpdateWalletAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>());
        await _economy.Received(1).AddTransactionAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
    }
}
