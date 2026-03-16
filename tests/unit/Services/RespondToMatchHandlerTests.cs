using FluentAssertions;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Domain.Interfaces;
using Loca.Services.Social.Commands;
using Loca.Services.Social.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Loca.Tests.Unit.Services;

public class RespondToMatchHandlerTests
{
    private readonly IMatchRepository _matches = Substitute.For<IMatchRepository>();
    private readonly ILogger<RespondToMatchHandler> _logger = Substitute.For<ILogger<RespondToMatchHandler>>();
    private readonly RespondToMatchHandler _handler;

    public RespondToMatchHandlerTests()
    {
        _handler = new RespondToMatchHandler(_matches, _logger);
    }

    [Fact]
    public async Task Should_ReturnError_When_RequestNotFound()
    {
        _matches.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((MatchRequest?)null);

        var cmd = new RespondToMatchCommand(Guid.NewGuid(), "accept") { UserId = Guid.NewGuid() };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Should_ReturnError_When_NotReceiver()
    {
        var request = new MatchRequest
        {
            SenderId = Guid.NewGuid(),
            ReceiverId = Guid.NewGuid(),
            Status = MatchRequestStatus.Pending
        };
        _matches.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(request);

        var cmd = new RespondToMatchCommand(request.Id, "accept") { UserId = Guid.NewGuid() };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Should_CreateConversation_When_Accepted()
    {
        var receiverId = Guid.NewGuid();
        var request = new MatchRequest
        {
            SenderId = Guid.NewGuid(),
            ReceiverId = receiverId,
            VenueId = Guid.NewGuid(),
            Status = MatchRequestStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        _matches.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(request);

        var cmd = new RespondToMatchCommand(request.Id, "accept") { UserId = receiverId };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ConversationId.Should().NotBeNull();
        await _matches.Received(1).AddConversationAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_SetDeclinedStatus_When_Declined()
    {
        var receiverId = Guid.NewGuid();
        var request = new MatchRequest
        {
            SenderId = Guid.NewGuid(),
            ReceiverId = receiverId,
            VenueId = Guid.NewGuid(),
            Status = MatchRequestStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        _matches.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(request);

        var cmd = new RespondToMatchCommand(request.Id, "decline") { UserId = receiverId };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.ConversationId.Should().BeNull();
        await _matches.Received(1).UpdateAsync(Arg.Is<MatchRequest>(r => r.Status == MatchRequestStatus.Declined), Arg.Any<CancellationToken>());
    }
}
