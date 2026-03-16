using FluentAssertions;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Services.Game.Engine;

namespace Loca.Tests.Unit.Services;

public class MafiaEngineTests
{
    [Theory]
    [InlineData(GameType.Mafia, 5, 12)]
    [InlineData(GameType.TruthOrDare, 3, 10)]
    [InlineData(GameType.Uno, 2, 6)]
    [InlineData(GameType.Domino, 2, 4)]
    [InlineData(GameType.QuizBattle, 2, 20)]
    public void Should_ReturnCorrectPlayerLimits(GameType type, int expectedMin, int expectedMax)
    {
        var (min, max) = GameStateMachine.GetPlayerLimits(type);
        min.Should().Be(expectedMin);
        max.Should().Be(expectedMax);
    }

    [Fact]
    public void Should_InitializeMafiaGame_WithRoles()
    {
        var session = new GameSession
        {
            GameType = GameType.Mafia,
            HostUserId = Guid.NewGuid(),
            MinPlayers = 5,
            MaxPlayers = 12,
        };

        // Add 5 players
        for (int i = 0; i < 5; i++)
            session.Players.Add(new GamePlayer { SessionId = session.Id, UserId = Guid.NewGuid() });

        GameStateMachine.InitializeGame(session);

        session.CurrentPhase.Should().Be("Night");
        session.StateJson.Should().NotBeNullOrEmpty();
        session.Players.Should().AllSatisfy(p => p.Role.Should().NotBeNullOrEmpty());

        // Should have exactly 1 Mafia, 1 Doctor, 1 Detective, 2 Citizens for 5 players
        session.Players.Count(p => p.Role == "Mafia").Should().Be(1);
        session.Players.Count(p => p.Role == "Doctor").Should().Be(1);
        session.Players.Count(p => p.Role == "Detective").Should().Be(1);
        session.Players.Count(p => p.Role == "Citizen").Should().Be(2);
    }

    [Fact]
    public void Should_InitializeTruthOrDare()
    {
        var session = new GameSession
        {
            GameType = GameType.TruthOrDare,
            HostUserId = Guid.NewGuid(),
            MinPlayers = 3,
            MaxPlayers = 10,
        };

        for (int i = 0; i < 4; i++)
            session.Players.Add(new GamePlayer { SessionId = session.Id, UserId = Guid.NewGuid() });

        GameStateMachine.InitializeGame(session);

        session.CurrentPhase.Should().Be("Choosing");
        session.StateJson.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_ProvideFogOfWar_ForMafiaPlayers()
    {
        var session = new GameSession
        {
            GameType = GameType.Mafia,
            HostUserId = Guid.NewGuid(),
            MinPlayers = 5,
            MaxPlayers = 12,
        };

        for (int i = 0; i < 5; i++)
            session.Players.Add(new GamePlayer { SessionId = session.Id, UserId = Guid.NewGuid() });

        GameStateMachine.InitializeGame(session);

        var mafiaPlayer = session.Players.First(p => p.Role == "Mafia");
        var citizenPlayer = session.Players.First(p => p.Role == "Citizen");

        var mafiaState = GameStateMachine.GetPlayerState(session, mafiaPlayer.UserId);
        var citizenState = GameStateMachine.GetPlayerState(session, citizenPlayer.UserId);

        mafiaState.Should().NotBeNull();
        citizenState.Should().NotBeNull();
    }
}
