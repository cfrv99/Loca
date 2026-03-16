using Loca.Application.DTOs;
using Loca.Application.Interfaces;
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
    private readonly LocaDbContext _db;
    private readonly IRedisService _redis;
    private readonly ILogger<GameHub> _logger;

    public GameHub(LocaDbContext db, IRedisService redis, ILogger<GameHub> logger)
    {
        _db = db;
        _redis = redis;
        _logger = logger;
    }

    public async Task CreateGame(string venueId, string gameType, int maxPlayers, object? settings)
    {
        var userId = Guid.Parse(GetUserId());
        var type = Enum.Parse<GameType>(gameType, true);

        var (min, max) = GameStateMachine.GetPlayerLimits(type);
        if (maxPlayers < min || maxPlayers > max) maxPlayers = max;

        var session = new GameSession
        {
            VenueId = Guid.Parse(venueId),
            GameType = type,
            HostUserId = userId,
            MinPlayers = min,
            MaxPlayers = maxPlayers,
        };
        session.Players.Add(new GamePlayer { SessionId = session.Id, UserId = userId });

        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();

        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{session.Id}");

        var dto = MapSessionDto(session);
        await Clients.Group($"venue_{venueId}").SendAsync("gameCreated", dto);
        _logger.LogInformation("Game {GameId} ({Type}) created by {UserId}", session.Id, type, userId);
    }

    public async Task JoinGame(string sessionId)
    {
        var userId = Guid.Parse(GetUserId());
        var session = await GetSessionWithPlayers(Guid.Parse(sessionId));
        if (session is null) return;

        if (session.Players.Count >= session.MaxPlayers)
        {
            await Clients.Caller.SendAsync("error", new { code = "GAME_FULL", message = "Oyun doludur" });
            return;
        }

        if (session.Players.Any(p => p.UserId == userId)) return;

        session.Players.Add(new GamePlayer { SessionId = session.Id, UserId = userId });
        await _db.SaveChangesAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{sessionId}");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var playerDto = new GamePlayerDto(userId, user?.DisplayName ?? "Guest", user?.AvatarUrl, 0, true, true);
        await Clients.Group($"game_{sessionId}").SendAsync("playerJoined", sessionId, playerDto);
    }

    public async Task StartGame(string sessionId)
    {
        var userId = Guid.Parse(GetUserId());
        var session = await GetSessionWithPlayers(Guid.Parse(sessionId));
        if (session is null) return;

        if (session.HostUserId != userId)
        {
            await Clients.Caller.SendAsync("error", new { code = "NOT_HOST", message = "Yalnız host oyunu başlada bilər" });
            return;
        }

        if (session.Players.Count < session.MinPlayers)
        {
            await Clients.Caller.SendAsync("error", new { code = "NOT_ENOUGH_PLAYERS", message = $"Minimum {session.MinPlayers} oyunçu lazımdır" });
            return;
        }

        session.Status = GameStatus.InProgress;
        session.StartedAt = DateTime.UtcNow;

        // Initialize game state based on type
        GameStateMachine.InitializeGame(session);
        await _db.SaveChangesAsync();

        // Send personalized state to each player (fog of war)
        foreach (var player in session.Players)
        {
            var state = GameStateMachine.GetPlayerState(session, player.UserId);
            // In a real implementation, we'd need to map ConnectionId per player
            await Clients.Group($"game_{sessionId}").SendAsync("gameStarted", sessionId, state);
        }

        _logger.LogInformation("Game {GameId} started with {PlayerCount} players", session.Id, session.Players.Count);
    }

    public async Task SubmitAction(string sessionId, GameActionDto action)
    {
        var userId = Guid.Parse(GetUserId());
        var session = await GetSessionWithPlayers(Guid.Parse(sessionId));
        if (session is null || session.Status != GameStatus.InProgress) return;

        var result = GameStateMachine.ProcessAction(session, userId, action);
        await _db.SaveChangesAsync();

        await Clients.Group($"game_{sessionId}").SendAsync("stateUpdated", sessionId, result);

        if (session.Status == GameStatus.Completed)
        {
            var gameResult = GameStateMachine.GetResult(session);
            await Clients.Group($"game_{sessionId}").SendAsync("gameEnded", sessionId, gameResult);
        }
    }

    public async Task LeaveGame(string sessionId)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game_{sessionId}");
        await Clients.Group($"game_{sessionId}").SendAsync("playerLeft", sessionId, userId);
    }

    private async Task<GameSession?> GetSessionWithPlayers(Guid id)
        => await _db.GameSessions.Include(s => s.Players).FirstOrDefaultAsync(s => s.Id == id);

    private string GetUserId() => Context.User?.FindFirst("sub")?.Value
        ?? throw new HubException("User not authenticated");

    private static GameSessionDto MapSessionDto(GameSession s) => new(
        s.Id, s.GameType.ToString(), s.HostUserId, s.MaxPlayers, s.MinPlayers, s.Status.ToString(),
        s.Players.Select(p => new GamePlayerDto(p.UserId, "", null, p.Score, p.IsAlive, p.IsConnected)).ToList()
    );
}
