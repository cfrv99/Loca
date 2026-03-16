using Loca.Application.DTOs;
using Loca.Application.Interfaces;
using Loca.Domain.Entities;
using Loca.Domain.Enums;
using Loca.Infrastructure.Persistence;
using Loca.Services.Game.Engine;
using Loca.Services.Social.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loca.Services.Game.Hubs;

/// <summary>
/// GameHub — /hubs/game
/// Manages game sessions: lobby, in-game actions, and results.
/// Auth: JWT Bearer required.
/// </summary>
[Authorize]
public class GameHub : Hub
{
    private readonly LocaDbContext _db;
    private readonly IRedisService _redis;
    private readonly IHubContext<VenueChatHub> _venueChatHubContext;
    private readonly ILogger<GameHub> _logger;

    /// <summary>
    /// Tracks disconnect timers for grace period reconnection (60s).
    /// Key = "{sessionId}_{userId}", Value = CancellationTokenSource.
    /// </summary>
    private static readonly Dictionary<string, CancellationTokenSource> _disconnectTimers = new();
    private static readonly object _timerLock = new();

    public GameHub(
        LocaDbContext db,
        IRedisService redis,
        IHubContext<VenueChatHub> venueChatHubContext,
        ILogger<GameHub> logger)
    {
        _db = db;
        _redis = redis;
        _venueChatHubContext = venueChatHubContext;
        _logger = logger;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Guid.Parse(GetUserId());

        // Find all active game sessions this user is in
        var activePlayers = await _db.GamePlayers
            .Include(gp => gp.Session)
            .Where(gp => gp.UserId == userId && gp.IsConnected &&
                         gp.Session != null && gp.Session.Status == GameStatus.InProgress)
            .ToListAsync();

        foreach (var player in activePlayers)
        {
            player.IsConnected = false;
            player.DisconnectedAt = DateTime.UtcNow;

            var sessionId = player.SessionId;
            await Clients.Group($"game_{sessionId}").SendAsync("playerDisconnected", sessionId.ToString(), userId.ToString());

            _logger.LogWarning("Player {UserId} disconnected from game {SessionId}. Starting 60s grace period.",
                userId, sessionId);

            // Start 60s grace period timer
            StartDisconnectGracePeriod(sessionId, userId);
        }

        await _db.SaveChangesAsync();
        await base.OnDisconnectedAsync(exception);
    }

    // ── Client → Server methods (PascalCase) ──

    /// <summary>
    /// Host creates a new game session. Returns sessionId via gameCreated event.
    /// </summary>
    public async Task CreateGame(string venueId, string gameType, int maxPlayers, object? settings)
    {
        var userId = Guid.Parse(GetUserId());

        if (!Enum.TryParse<GameType>(gameType, true, out var type))
        {
            await Clients.Caller.SendAsync("error", new { code = "INVALID_GAME_TYPE", message = "Yanlış oyun növü" });
            return;
        }

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
        _logger.LogInformation("Game {GameId} ({Type}) created by {UserId} at venue {VenueId}",
            session.Id, type, userId, venueId);
    }

    /// <summary>
    /// Join a game lobby. Handles reconnection if player was previously disconnected.
    /// </summary>
    public async Task JoinGame(string sessionId)
    {
        var userId = Guid.Parse(GetUserId());
        var sessionGuid = Guid.Parse(sessionId);
        var session = await GetSessionWithPlayers(sessionGuid);

        if (session is null)
        {
            await Clients.Caller.SendAsync("error", new { code = "GAME_NOT_FOUND", message = "Oyun tapılmadı" });
            return;
        }

        // Check if this is a reconnection
        var existingPlayer = session.Players.FirstOrDefault(p => p.UserId == userId);
        if (existingPlayer is not null)
        {
            if (!existingPlayer.IsConnected)
            {
                // Cancel the disconnect timer
                CancelDisconnectTimer(sessionGuid, userId);

                existingPlayer.IsConnected = true;
                existingPlayer.DisconnectedAt = null;
                await _db.SaveChangesAsync();

                await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{sessionId}");
                await Clients.Group($"game_{sessionId}").SendAsync("playerReconnected", sessionId, userId.ToString());

                // Send current game state to reconnected player
                if (session.Status == GameStatus.InProgress)
                {
                    var state = GameStateMachine.GetPlayerState(session, userId);
                    await Clients.Caller.SendAsync("stateUpdated", sessionId, state);
                }

                _logger.LogInformation("Player {UserId} reconnected to game {SessionId}", userId, sessionId);
            }
            return; // Already in the game
        }

        // New join
        if (session.Status != GameStatus.Lobby)
        {
            await Clients.Caller.SendAsync("error", new { code = "GAME_STARTED", message = "Oyun artıq başlayıb" });
            return;
        }

        if (session.Players.Count >= session.MaxPlayers)
        {
            await Clients.Caller.SendAsync("error", new { code = "GAME_FULL", message = "Oyun doludur" });
            return;
        }

        session.Players.Add(new GamePlayer { SessionId = session.Id, UserId = userId });
        await _db.SaveChangesAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game_{sessionId}");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var playerDto = new GamePlayerDto(userId, user?.DisplayName ?? "Guest", user?.AvatarUrl, 0, true, true);
        await Clients.Group($"game_{sessionId}").SendAsync("playerJoined", sessionId, playerDto);
    }

    /// <summary>
    /// Host starts the game. Requires minimum players met.
    /// Sends personalized state to each player (fog of war).
    /// </summary>
    public async Task StartGame(string sessionId)
    {
        var userId = Guid.Parse(GetUserId());
        var session = await GetSessionWithPlayers(Guid.Parse(sessionId));

        if (session is null)
        {
            await Clients.Caller.SendAsync("error", new { code = "GAME_NOT_FOUND", message = "Oyun tapılmadı" });
            return;
        }

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
            // Broadcast to group; in production, use connection mapping for per-player state
            await Clients.Group($"game_{sessionId}").SendAsync("gameStarted", sessionId, state);
        }

        _logger.LogInformation("Game {GameId} started with {PlayerCount} players", session.Id, session.Players.Count);
    }

    /// <summary>
    /// Submit an in-game action (vote, play card, answer question, etc.).
    /// Server validates and processes the action, then broadcasts updated state.
    /// </summary>
    public async Task SubmitAction(string sessionId, GameActionDto action)
    {
        var userId = Guid.Parse(GetUserId());
        var session = await GetSessionWithPlayers(Guid.Parse(sessionId));

        if (session is null)
        {
            await Clients.Caller.SendAsync("error", new { code = "GAME_NOT_FOUND", message = "Oyun tapılmadı" });
            return;
        }

        if (session.Status != GameStatus.InProgress)
        {
            await Clients.Caller.SendAsync("error", new { code = "GAME_NOT_ACTIVE", message = "Oyun aktiv deyil" });
            return;
        }

        var player = session.Players.FirstOrDefault(p => p.UserId == userId);
        if (player is null || !player.IsAlive)
        {
            await Clients.Caller.SendAsync("error", new { code = "CANNOT_ACT", message = "Bu hərəkəti edə bilməzsiniz" });
            return;
        }

        var result = GameStateMachine.ProcessAction(session, userId, action);
        await _db.SaveChangesAsync();

        await Clients.Group($"game_{sessionId}").SendAsync("stateUpdated", sessionId, result);

        // Check if game ended
        if (session.Status == GameStatus.Completed)
        {
            session.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var gameResult = GameStateMachine.GetResult(session);
            await Clients.Group($"game_{sessionId}").SendAsync("gameEnded", sessionId, gameResult);

            // Auto-post game result to venue chat as a system message
            await PostGameResultToVenueChat(session, gameResult);

            _logger.LogInformation("Game {GameId} completed. Winner: {WinnerId}", session.Id, gameResult.WinnerId);
        }
    }

    /// <summary>
    /// Leave a game lobby or forfeit in-game.
    /// </summary>
    public async Task LeaveGame(string sessionId)
    {
        var userId = GetUserId();
        var userGuid = Guid.Parse(userId);
        var session = await GetSessionWithPlayers(Guid.Parse(sessionId));

        if (session is null) return;

        // Cancel any disconnect timer
        CancelDisconnectTimer(session.Id, userGuid);

        // Remove from group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game_{sessionId}");

        var player = session.Players.FirstOrDefault(p => p.UserId == userGuid);
        if (player is not null)
        {
            if (session.Status == GameStatus.Lobby)
            {
                // In lobby, fully remove the player
                _db.GamePlayers.Remove(player);
            }
            else
            {
                // In-game, mark as not alive (forfeited)
                player.IsAlive = false;
                player.IsConnected = false;
            }

            await _db.SaveChangesAsync();
        }

        await Clients.Group($"game_{sessionId}").SendAsync("playerLeft", sessionId, userId);
    }

    // ── Private helpers ──

    /// <summary>
    /// Posts the game result as a system message in the venue's public chat.
    /// </summary>
    private async Task PostGameResultToVenueChat(GameSession session, GameResultDto result)
    {
        var gameTypeName = session.GameType switch
        {
            GameType.Mafia => "Mafia",
            GameType.TruthOrDare => "Həqiqət və ya Cəsarət",
            GameType.Uno => "Uno",
            GameType.Domino => "Domino",
            GameType.QuizBattle => "Quiz Battle",
            GameType.WouldYouRather => "Nəyi Seçərdin",
            _ => session.GameType.ToString()
        };

        string content;
        if (result.WinnerId.HasValue)
        {
            var winner = await _db.Users.FirstOrDefaultAsync(u => u.Id == result.WinnerId.Value);
            var winnerName = winner?.DisplayName ?? "Guest";
            content = $"Oyun bitdi! {gameTypeName} - Qalib: {winnerName}! Oyun müddəti: {result.Duration.Minutes} dəqiqə.";
        }
        else
        {
            content = $"Oyun bitdi! {gameTypeName} - Oyun müddəti: {result.Duration.Minutes} dəqiqə.";
        }

        await VenueChatHub.BroadcastSystemMessageAsync(
            _venueChatHubContext,
            session.VenueId.ToString(),
            content);

        _logger.LogInformation("Game result posted to venue {VenueId} chat for game {GameId}",
            session.VenueId, session.Id);
    }

    /// <summary>
    /// Starts a 60-second grace period for a disconnected player.
    /// If they don't reconnect, they are removed from the game.
    /// </summary>
    private void StartDisconnectGracePeriod(Guid sessionId, Guid userId)
    {
        var key = $"{sessionId}_{userId}";

        lock (_timerLock)
        {
            // Cancel any existing timer
            if (_disconnectTimers.TryGetValue(key, out var existing))
            {
                existing.Cancel();
                existing.Dispose();
            }

            var cts = new CancellationTokenSource();
            _disconnectTimers[key] = cts;

            // Fire and forget the grace period check
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), cts.Token);

                    // Grace period expired — handle timeout
                    _logger.LogWarning("Grace period expired for player {UserId} in game {SessionId}. Applying timeout.",
                        userId, sessionId);

                    await Clients.Group($"game_{sessionId}").SendAsync("turnTimeout", sessionId.ToString(), userId.ToString());
                }
                catch (TaskCanceledException)
                {
                    // Player reconnected before timeout — timer cancelled
                }
                finally
                {
                    lock (_timerLock)
                    {
                        _disconnectTimers.Remove(key);
                    }
                }
            }, cts.Token);
        }
    }

    /// <summary>
    /// Cancels a disconnect grace period timer (player reconnected).
    /// </summary>
    private static void CancelDisconnectTimer(Guid sessionId, Guid userId)
    {
        var key = $"{sessionId}_{userId}";
        lock (_timerLock)
        {
            if (_disconnectTimers.TryGetValue(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _disconnectTimers.Remove(key);
            }
        }
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
