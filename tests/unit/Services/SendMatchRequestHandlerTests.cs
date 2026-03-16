using FluentAssertions;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Social.Commands;
using Loca.Services.Social.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class SendMatchRequestHandlerTests
{
    private readonly IMatchRepository _matches = Substitute.For<IMatchRepository>();
    private readonly ICheckInRepository _checkIns = Substitute.For<ICheckInRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ILogger<SendMatchRequestHandler> _logger = Substitute.For<ILogger<SendMatchRequestHandler>>();
    private readonly SendMatchRequestHandler _handler;

    public SendMatchRequestHandlerTests()
    {
        _handler = new SendMatchRequestHandler(_matches, _checkIns, _users, _logger);
    }

    [Fact]
    public async Task Should_ReturnError_When_SenderNotCheckedIn()
    {
        _matches.IsBlockedAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _checkIns.GetActiveCheckInByUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CheckIn?)null);

        var cmd = new SendMatchRequestCommand(Guid.NewGuid(), "Salam!")
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("NOT_CHECKED_IN");
    }

    [Fact]
    public async Task Should_ReturnError_When_SenderIsAnonymous()
    {
        var venueId = Guid.NewGuid();
        _matches.IsBlockedAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _checkIns.GetActiveCheckInByUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new CheckIn { VenueId = venueId, IsAnonymous = true });

        var cmd = new SendMatchRequestCommand(Guid.NewGuid(), null)
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("ANONYMOUS_NOT_ALLOWED");
    }

    [Fact]
    public async Task Should_ReturnError_When_ReceiverNotAtSameVenue()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        _matches.IsBlockedAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _checkIns.GetActiveCheckInByUserAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(new CheckIn { UserId = senderId, VenueId = venueId, IsAnonymous = false });
        _checkIns.GetActiveCheckInAsync(receiverId, venueId, Arg.Any<CancellationToken>())
            .Returns((CheckIn?)null);
        _matches.GetDailyRequestCountAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(0);
        _users.GetByIdAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(new User { Id = senderId, IsPremium = false });

        var cmd = new SendMatchRequestCommand(receiverId, null) { UserId = senderId };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("SAME_VENUE_REQUIRED");
    }

    [Fact]
    public async Task Should_ReturnError_When_DailyLimitReached()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        _matches.IsBlockedAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _checkIns.GetActiveCheckInByUserAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(new CheckIn { UserId = senderId, VenueId = venueId, IsAnonymous = false });
        _checkIns.GetActiveCheckInAsync(receiverId, venueId, Arg.Any<CancellationToken>())
            .Returns(new CheckIn { UserId = receiverId, VenueId = venueId, IsAnonymous = false });
        _matches.GetDailyRequestCountAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(5);
        _users.GetByIdAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(new User { Id = senderId, IsPremium = false });

        var cmd = new SendMatchRequestCommand(receiverId, null) { UserId = senderId };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("DAILY_LIMIT_REACHED");
    }

    [Fact]
    public async Task Should_SuccessfullySendRequest_When_AllValidationsPass()
    {
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var venueId = Guid.NewGuid();

        _matches.IsBlockedAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _checkIns.GetActiveCheckInByUserAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(new CheckIn { UserId = senderId, VenueId = venueId, IsAnonymous = false });
        _checkIns.GetActiveCheckInAsync(receiverId, venueId, Arg.Any<CancellationToken>())
            .Returns(new CheckIn { UserId = receiverId, VenueId = venueId, IsAnonymous = false });
        _matches.GetDailyRequestCountAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(0);
        _matches.GetPendingAsync(senderId, receiverId, Arg.Any<CancellationToken>())
            .Returns((MatchRequest?)null);
        _matches.GetDeclineCountAsync(senderId, receiverId, Arg.Any<CancellationToken>())
            .Returns(0);
        _users.GetByIdAsync(senderId, Arg.Any<CancellationToken>())
            .Returns(new User { Id = senderId, DisplayName = "Ali", IsPremium = false });

        var cmd = new SendMatchRequestCommand(receiverId, "Salam!") { UserId = senderId };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SenderId.Should().Be(senderId);
        result.Value.Status.Should().Be("Pending");
        result.Value.IntroMessage.Should().Be("Salam!");
    }

    [Fact]
    public async Task Should_ReturnError_When_UsersBlocked()
    {
        _matches.IsBlockedAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var cmd = new SendMatchRequestCommand(Guid.NewGuid(), null)
        {
            UserId = Guid.NewGuid()
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("USER_BLOCKED");
    }
}
