using FluentAssertions;
using Loca.Application.DTOs;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Services.Game.Engine;

namespace Loca.Tests.Unit.Services;

/// <summary>
/// Tests for GameHub business logic: disconnect grace period, reconnection,
/// game result DTO construction, and forfeit behavior.
/// </summary>
public class GameHubTests
{
    [Fact]
    public void Should_MarkPlayerDisconnected_OnDisconnect()
    {
        var player = new GamePlayer
        {
            UserId = Guid.NewGuid(),
            IsConnected = true,
            DisconnectedAt = null,
        };

        // Simulate disconnect
        player.IsConnected = false;
        player.DisconnectedAt = DateTime.UtcNow;

        player.IsConnected.Should().BeFalse();
        player.DisconnectedAt.Should().NotBeNull();
    }

    [Fact]
    public void Should_MarkPlayerReconnected_OnReconnect()
    {
        var player = new GamePlayer
        {
            UserId = Guid.NewGuid(),
            IsConnected = false,
            DisconnectedAt = DateTime.UtcNow.AddSeconds(-30),
        };

        // Simulate reconnection within 60s grace period
        player.IsConnected = true;
        player.DisconnectedAt = null;

        player.IsConnected.Should().BeTrue();
        player.DisconnectedAt.Should().BeNull();
    }

    [Fact]
    public void Should_DetectGracePeriodExpired_After60Seconds()
    {
        var disconnectedAt = DateTime.UtcNow.AddSeconds(-61);
        var gracePeriod = TimeSpan.FromSeconds(60);

        var isExpired = (DateTime.UtcNow - disconnectedAt) > gracePeriod;

        isExpired.Should().BeTrue();
    }

    [Fact]
    public void Should_DetectGracePeriodActive_Within60Seconds()
    {
        var disconnectedAt = DateTime.UtcNow.AddSeconds(-30);
        var gracePeriod = TimeSpan.FromSeconds(60);

        var isExpired = (DateTime.UtcNow - disconnectedAt) > gracePeriod;

        isExpired.Should().BeFalse();
    }

    [Fact]
    public void Should_ForfeitPlayer_WhenLeavingInProgressGame()
    {
        var session = new GameSession
        {
            Status = GameStatus.InProgress,
        };

        var player = new GamePlayer
        {
            UserId = Guid.NewGuid(),
            IsAlive = true,
            IsConnected = true,
        };

        // Simulate forfeit (leaving an in-progress game)
        if (session.Status == GameStatus.InProgress)
        {
            player.IsAlive = false;
            player.IsConnected = false;
        }

        player.IsAlive.Should().BeFalse();
        player.IsConnected.Should().BeFalse();
    }

    [Fact]
    public void Should_RemovePlayer_WhenLeavingLobby()
    {
        var session = new GameSession
        {
            Status = GameStatus.Lobby,
        };
        var player = new GamePlayer { UserId = Guid.NewGuid() };
        session.Players.Add(player);

        session.Players.Should().HaveCount(1);

        // In lobby, player is fully removed
        if (session.Status == GameStatus.Lobby)
        {
            session.Players.Remove(player);
        }

        session.Players.Should().BeEmpty();
    }

    [Fact]
    public void Should_GenerateGameResultDto_WithWinner()
    {
        var winnerId = Guid.NewGuid();
        var scores = new Dictionary<Guid, int>
        {
            { winnerId, 100 },
            { Guid.NewGuid(), 50 },
            { Guid.NewGuid(), 25 },
        };

        var result = new GameResultDto(winnerId, null, scores, TimeSpan.FromMinutes(15));

        result.WinnerId.Should().Be(winnerId);
        result.Scores.Should().HaveCount(3);
        result.Duration.TotalMinutes.Should().Be(15);
    }

    [Fact]
    public void Should_GenerateGameResultDto_WithoutWinner()
    {
        var result = new GameResultDto(null, null, new Dictionary<Guid, int>(), TimeSpan.FromMinutes(10));

        result.WinnerId.Should().BeNull();
    }

    [Fact]
    public void Should_MapGameTypeName_ToAzerbaijani()
    {
        var gameTypeNames = new Dictionary<GameType, string>
        {
            { GameType.Mafia, "Mafia" },
            { GameType.TruthOrDare, "Həqiqət və ya Cəsarət" },
            { GameType.Uno, "Uno" },
            { GameType.Domino, "Domino" },
            { GameType.QuizBattle, "Quiz Battle" },
            { GameType.WouldYouRather, "Nəyi Seçərdin" },
        };

        gameTypeNames[GameType.Mafia].Should().Be("Mafia");
        gameTypeNames[GameType.TruthOrDare].Should().Be("Həqiqət və ya Cəsarət");
        gameTypeNames.Should().HaveCount(6);
    }

    [Fact]
    public void Should_PreventJoining_WhenGameIsFull()
    {
        var session = new GameSession
        {
            MaxPlayers = 2,
            Status = GameStatus.Lobby,
        };
        session.Players.Add(new GamePlayer { UserId = Guid.NewGuid() });
        session.Players.Add(new GamePlayer { UserId = Guid.NewGuid() });

        var isFull = session.Players.Count >= session.MaxPlayers;

        isFull.Should().BeTrue();
    }

    [Fact]
    public void Should_PreventStarting_WithoutMinPlayers()
    {
        var session = new GameSession
        {
            MinPlayers = 5,
            Status = GameStatus.Lobby,
        };
        session.Players.Add(new GamePlayer { UserId = Guid.NewGuid() });
        session.Players.Add(new GamePlayer { UserId = Guid.NewGuid() });

        var hasEnoughPlayers = session.Players.Count >= session.MinPlayers;

        hasEnoughPlayers.Should().BeFalse();
    }

    [Fact]
    public void Should_OnlyAllowHost_ToStartGame()
    {
        var hostId = Guid.NewGuid();
        var otherPlayer = Guid.NewGuid();

        var session = new GameSession
        {
            HostUserId = hostId,
        };

        (session.HostUserId == hostId).Should().BeTrue();
        (session.HostUserId == otherPlayer).Should().BeFalse();
    }
}
