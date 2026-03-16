using System.Text.Json;
using Loca.Application.DTOs;
using Loca.Domain.Entities;
using Loca.Domain.Enums;

namespace Loca.Services.Game.Engine;

public static class GameStateMachine
{
    public static (int min, int max) GetPlayerLimits(GameType type) => type switch
    {
        GameType.Mafia => (5, 12),
        GameType.TruthOrDare => (3, 10),
        GameType.Uno => (2, 6),
        GameType.Domino => (2, 4),
        GameType.QuizBattle => (2, 20),
        GameType.WouldYouRather => (2, 20),
        _ => (2, 10)
    };

    public static void InitializeGame(GameSession session)
    {
        switch (session.GameType)
        {
            case GameType.Mafia:
                InitializeMafia(session);
                break;
            case GameType.TruthOrDare:
                InitializeTruthOrDare(session);
                break;
            default:
                session.StateJson = JsonSerializer.Serialize(new { round = 1, turn = 0 });
                break;
        }
    }

    private static void InitializeMafia(GameSession session)
    {
        var players = session.Players.ToList();
        var roles = AssignMafiaRoles(players.Count);
        var shuffled = players.OrderBy(_ => Random.Shared.Next()).ToList();

        for (int i = 0; i < shuffled.Count; i++)
        {
            shuffled[i].Role = roles[i].ToString();
            shuffled[i].IsAlive = true;
        }

        session.CurrentPhase = MafiaPhase.Night.ToString();
        session.PhaseDeadline = DateTime.UtcNow.AddSeconds(45);
        session.StateJson = JsonSerializer.Serialize(new
        {
            phase = "Night",
            round = 1,
            mafiaTarget = (string?)null,
            doctorTarget = (string?)null,
            detectiveTarget = (string?)null,
            votes = new Dictionary<string, string>()
        });
    }

    private static void InitializeTruthOrDare(GameSession session)
    {
        var players = session.Players.ToList();
        var currentTurn = Random.Shared.Next(0, players.Count);

        session.CurrentPhase = "Choosing";
        session.PhaseDeadline = DateTime.UtcNow.AddSeconds(60);
        session.StateJson = JsonSerializer.Serialize(new
        {
            currentPlayerIndex = currentTurn,
            currentPlayerId = players[currentTurn].UserId,
            round = 1,
        });
    }

    private static List<MafiaRole> AssignMafiaRoles(int playerCount) => playerCount switch
    {
        5 => new() { MafiaRole.Mafia, MafiaRole.Doctor, MafiaRole.Detective, MafiaRole.Citizen, MafiaRole.Citizen },
        6 => new() { MafiaRole.Mafia, MafiaRole.Mafia, MafiaRole.Doctor, MafiaRole.Detective, MafiaRole.Citizen, MafiaRole.Citizen },
        7 => new() { MafiaRole.Mafia, MafiaRole.Mafia, MafiaRole.Doctor, MafiaRole.Detective, MafiaRole.Citizen, MafiaRole.Citizen, MafiaRole.Citizen },
        8 => new() { MafiaRole.Mafia, MafiaRole.Mafia, MafiaRole.Doctor, MafiaRole.Detective, MafiaRole.Citizen, MafiaRole.Citizen, MafiaRole.Citizen, MafiaRole.Citizen },
        _ => Enumerable.Range(0, playerCount).Select(i => i < playerCount / 4 ? MafiaRole.Mafia :
            i == playerCount / 4 ? MafiaRole.Doctor : i == playerCount / 4 + 1 ? MafiaRole.Detective : MafiaRole.Citizen).ToList()
    };

    public static object GetPlayerState(GameSession session, Guid playerId)
    {
        var player = session.Players.FirstOrDefault(p => p.UserId == playerId);
        if (player is null) return new { };

        return session.GameType switch
        {
            GameType.Mafia => GetMafiaPlayerState(session, player),
            _ => new { role = player.Role, score = player.Score, phase = session.CurrentPhase }
        };
    }

    private static object GetMafiaPlayerState(GameSession session, GamePlayer player)
    {
        var role = Enum.Parse<MafiaRole>(player.Role!);
        var alivePlayers = session.Players.Where(p => p.IsAlive).Select(p => new { p.UserId, p.IsAlive }).ToList();

        // Mafia players can see other mafia
        var teamMates = role == MafiaRole.Mafia
            ? session.Players.Where(p => p.Role == MafiaRole.Mafia.ToString() && p.UserId != player.UserId)
                .Select(p => p.UserId).ToList()
            : new List<Guid>();

        return new
        {
            role = player.Role,
            isAlive = player.IsAlive,
            phase = session.CurrentPhase,
            phaseDeadline = session.PhaseDeadline,
            alivePlayers,
            teamMates
        };
    }

    public static object ProcessAction(GameSession session, Guid userId, GameActionDto action)
    {
        // Simplified action processing - in production this would be much more complex
        return new { processed = true, actionType = action.ActionType };
    }

    public static GameResultDto GetResult(GameSession session)
    {
        var scores = session.Players.ToDictionary(p => p.UserId, p => p.Score);
        var winner = session.Players.OrderByDescending(p => p.Score).FirstOrDefault();
        return new GameResultDto(
            WinnerId: winner?.UserId,
            WinnerTeam: null,
            Scores: scores,
            Duration: session.CompletedAt.HasValue && session.StartedAt.HasValue
                ? session.CompletedAt.Value - session.StartedAt.Value
                : TimeSpan.Zero
        );
    }
}
