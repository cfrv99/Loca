using FluentAssertions;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Interfaces;
using Loca.Services.Venue.Commands;
using Loca.Services.Venue.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class CheckOutHandlerTests
{
    private readonly ICheckInRepository _checkIns = Substitute.For<ICheckInRepository>();
    private readonly IRedisService _redis = Substitute.For<IRedisService>();
    private readonly ILogger<CheckOutHandler> _logger = Substitute.For<ILogger<CheckOutHandler>>();
    private readonly CheckOutHandler _handler;

    public CheckOutHandlerTests()
    {
        _handler = new CheckOutHandler(_checkIns, _redis, _logger);
    }

    [Fact]
    public async Task Should_ReturnError_When_CheckInNotFound()
    {
        _checkIns.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((CheckIn?)null);

        var cmd = new CheckOutCommand(Guid.NewGuid()) { UserId = Guid.NewGuid() };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Should_ReturnError_When_NotOwnCheckIn()
    {
        var checkIn = new CheckIn
        {
            UserId = Guid.NewGuid(),
            VenueId = Guid.NewGuid()
        };
        _checkIns.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(checkIn);

        var cmd = new CheckOutCommand(checkIn.Id) { UserId = Guid.NewGuid() };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Should_ReturnError_When_AlreadyCheckedOut()
    {
        var userId = Guid.NewGuid();
        var checkIn = new CheckIn
        {
            UserId = userId,
            VenueId = Guid.NewGuid(),
            CheckOutAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _checkIns.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(checkIn);

        var cmd = new CheckOutCommand(checkIn.Id) { UserId = userId };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("ALREADY_CHECKED_OUT");
    }

    [Fact]
    public async Task Should_SuccessfullyCheckOut_When_Valid()
    {
        var userId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var checkIn = new CheckIn
        {
            UserId = userId,
            VenueId = venueId,
            CheckInAt = DateTime.UtcNow.AddHours(-1)
        };
        _checkIns.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(checkIn);

        var cmd = new CheckOutCommand(checkIn.Id) { UserId = userId };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Success.Should().BeTrue();
        await _redis.Received(1).DecrementVenueCountAsync(venueId, Arg.Any<string>());
        await _redis.Received(1).RemoveActiveUserAsync(venueId, userId);
        await _redis.Received(1).RemoveUserFromVenueAsync(userId, venueId);
    }
}
