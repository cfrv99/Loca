using FluentAssertions;
using Loca.Domain.Enums;
using Loca.Services.Game.Engine;

namespace Loca.Tests.Unit.Services;

public class MafiaEngineTests
{
    private readonly MafiaEngine _engine = new();

    [Fact]
    public void Should_AssignRolesToAllPlayers_When_GameInitialized()
    {
        var playerIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();

        var state = (MafiaState)_engine.InitializeState(playerIds);

        state.Roles.Should().HaveCount(6);
        state.AlivePlayers.Should().HaveCount(6);
        state.Phase.Should().Be(MafiaPhase.Day);
        state.Round.Should().Be(1);

        // With 6 players: 1 mafia, 1 doctor, 1 detective, 3 civilians
        state.Roles.Values.Count(r => r == MafiaRole.Mafia).Should().Be(1);
        state.Roles.Values.Count(r => r == MafiaRole.Doctor).Should().Be(1);
        state.Roles.Values.Count(r => r == MafiaRole.Detective).Should().Be(1);
        state.Roles.Values.Count(r => r == MafiaRole.Civilian).Should().Be(3);
    }

    [Fact]
    public void Should_EliminatePlayer_When_VotingCompletes()
    {
        var playerIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();
        var state = (MafiaState)_engine.InitializeState(playerIds);
        var target = playerIds[0];

        // All players vote for the same target
        foreach (var playerId in playerIds)
        {
            state = (MafiaState)_engine.ProcessAction(state, playerId, "vote", target.ToString());
        }

        state.AlivePlayers.Should().NotContain(target);
        state.EliminatedPlayers.Should().Contain(target);
        state.Phase.Should().Be(MafiaPhase.Night);
    }

    [Fact]
    public void Should_GameNotBeOver_When_MafiaAndCiviliansRemain()
    {
        var playerIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();
        var state = (MafiaState)_engine.InitializeState(playerIds);

        _engine.IsGameOver(state).Should().BeFalse();
    }

    [Fact]
    public void Should_GameBeOver_When_AllMafiaEliminated()
    {
        var playerIds = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();
        var state = (MafiaState)_engine.InitializeState(playerIds);

        // Remove all mafia from alive players
        var mafiaPlayers = state.Roles.Where(r => r.Value == MafiaRole.Mafia).Select(r => r.Key).ToList();
        foreach (var mafia in mafiaPlayers)
        {
            state.AlivePlayers.Remove(mafia);
        }

        _engine.IsGameOver(state).Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnCorrectGameType()
    {
        _engine.GameType.Should().Be(GameType.Mafia);
    }
}
