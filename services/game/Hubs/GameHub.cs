using System.Security.Claims;
using System.Text.Json;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Infrastructure.Persistence;
using Loca.Services.Game.Engine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Game.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly LocaDbContext _context;
    private readonly ILogger<GameHub> _logger;
    private readonly Dictionary<GameType, IGameEngine> _engines;

    public GameHub(LocaDbContext context, ILogger<GameHub> logger)
    {
        _context = context;
        _logger = logger;
        _engines = new Dictionary<GameType, IGameEngine>
        {
            { GameType.Mafia, new MafiaEngine() },
            { GameType.TruthOrDare, new TruthOrDareEngine() }
        };
    }

    private Guid GetUserId() => Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>
    /// Create a new game session in a venue
    /// </summary>
    public async Task CreateGame(Guid venueId, string gameType, int minPlayers, int maxPlayers)
    {
        var userId = GetUserId();
        if (!Enum.TryParse<GameType>(gameType, true, out var type))
        {
            await Clients.Caller.SendAsync("error", "Invalid game type");
            return;
        }

        var session = new GameSession
        {
            VenueId = venueId,
            HostId = userId,
            GameType = type,
            MinPlayers = Math.Max(minPlayers, 3),
            MaxPlayers = Math.Min(maxPlayers, 20),
            Players = new List<GamePlayer>
            {
                new() { UserId = userId, Status = GamePlayerStatus.Ready }
            }
        };

        _context.GameSessions.Add(session);
        await _context.SaveChangesAsync();

        var groupName = $"game_{session.Id}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group($"venue_{venueId}").SendAsync("gameCreated", new
        {
            sessionId = session.Id,
            gameType,
            hostId = userId,
            minPlayers = session.MinPlayers,
            maxPlayers = session.MaxPlayers
        });

        _logger.LogInformation("Game {GameType} created by {UserId} in venue {VenueId}", gameType, userId, venueId);
    }

    /// <summary>
    /// Join an existing game session
    /// </summary>
    public async Task JoinGame(Guid sessionId)
    {
        var userId = GetUserId();
        var session = await _context.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == GameSessionStatus.Lobby);

        if (session is null)
        {
            await Clients.Caller.SendAsync("error", "Game not found or already started");
            return;
        }

        if (session.Players.Count >= session.MaxPlayers)
        {
            await Clients.Caller.SendAsync("error", "Game is full");
            return;
        }

        if (session.Players.Any(p => p.UserId == userId))
        {
            await Clients.Caller.SendAsync("error", "Already in this game");
            return;
        }

        session.Players.Add(new GamePlayer { UserId = userId, Status = GamePlayerStatus.Ready });
        await _context.SaveChangesAsync();

        var groupName = $"game_{sessionId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("playerJoined", new
        {
            userId,
            playerCount = session.Players.Count
        });
    }

    /// <summary>
    /// Start the game (host only)
    /// </summary>
    public async Task StartGame(Guid sessionId)
    {
        var userId = GetUserId();
        var session = await _context.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null || session.HostId != userId || session.Status != GameSessionStatus.Lobby)
        {
            await Clients.Caller.SendAsync("error", "Cannot start game");
            return;
        }

        if (session.Players.Count < session.MinPlayers)
        {
            await Clients.Caller.SendAsync("error", $"Need at least {session.MinPlayers} players");
            return;
        }

        if (!_engines.TryGetValue(session.GameType, out var engine))
        {
            await Clients.Caller.SendAsync("error", "Game engine not available");
            return;
        }

        var playerIds = session.Players.Select(p => p.UserId).ToList();
        var initialState = engine.InitializeState(playerIds);

        session.Status = GameSessionStatus.InProgress;
        session.StartedAt = DateTime.UtcNow;
        session.GameState = JsonSerializer.Serialize(initialState);
        session.Players.ForEach(p => p.Status = GamePlayerStatus.Playing);
        await _context.SaveChangesAsync();

        var groupName = $"game_{sessionId}";

        // Send fog-of-war state to each player
        foreach (var player in session.Players)
        {
            var playerState = GetPlayerVisibleState(session.GameType, initialState, player.UserId);
            await Clients.User(player.UserId.ToString()).SendAsync("gameStarted", playerState);
        }

        _logger.LogInformation("Game {SessionId} started with {PlayerCount} players", sessionId, playerIds.Count);
    }

    /// <summary>
    /// Perform a game action
    /// </summary>
    public async Task GameAction(Guid sessionId, string action, string? data)
    {
        var userId = GetUserId();
        var session = await _context.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == GameSessionStatus.InProgress);

        if (session is null)
        {
            await Clients.Caller.SendAsync("error", "Game not found or not in progress");
            return;
        }

        if (!_engines.TryGetValue(session.GameType, out var engine))
            return;

        var state = DeserializeState(session.GameType, session.GameState!);
        var newState = engine.ProcessAction(state, userId, action, data);

        session.GameState = JsonSerializer.Serialize(newState);

        if (engine.IsGameOver(newState))
        {
            session.Status = GameSessionStatus.Completed;
            session.EndedAt = DateTime.UtcNow;
            var scores = engine.GetScores(newState);
            foreach (var player in session.Players)
            {
                player.Score = scores.GetValueOrDefault(player.UserId);
                player.Status = GamePlayerStatus.Waiting;
            }

            await _context.SaveChangesAsync();

            var groupName = $"game_{sessionId}";
            await Clients.Group(groupName).SendAsync("gameEnded", new
            {
                scores = scores.Select(s => new { userId = s.Key, score = s.Value })
            });
            return;
        }

        await _context.SaveChangesAsync();

        // Send updated state to each player (fog of war)
        foreach (var player in session.Players.Where(p => p.Status == GamePlayerStatus.Playing))
        {
            var playerState = GetPlayerVisibleState(session.GameType, newState, player.UserId);
            await Clients.User(player.UserId.ToString()).SendAsync("gameStateUpdated", playerState);
        }
    }

    private static object GetPlayerVisibleState(GameType gameType, object state, Guid playerId)
    {
        if (gameType == GameType.Mafia && state is MafiaState mafiaState)
        {
            return new
            {
                phase = mafiaState.Phase.ToString(),
                round = mafiaState.Round,
                alivePlayers = mafiaState.AlivePlayers,
                eliminatedPlayers = mafiaState.EliminatedPlayers,
                myRole = mafiaState.Roles.GetValueOrDefault(playerId).ToString(),
                voteCount = mafiaState.Votes.Count,
                totalAlive = mafiaState.AlivePlayers.Count
            };
        }

        return state;
    }

    private static object DeserializeState(GameType gameType, string json)
    {
        return gameType switch
        {
            GameType.Mafia => JsonSerializer.Deserialize<MafiaState>(json)!,
            GameType.TruthOrDare => JsonSerializer.Deserialize<TruthOrDareState>(json)!,
            _ => throw new NotSupportedException($"Game type {gameType} not supported")
        };
    }
}
