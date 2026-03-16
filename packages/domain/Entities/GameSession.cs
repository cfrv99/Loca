using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class GameSession : BaseEntity
{
    public Guid VenueId { get; set; }
    public GameType GameType { get; set; }
    public Guid HostUserId { get; set; }
    public GameStatus Status { get; set; } = GameStatus.Lobby;
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public string? SettingsJson { get; set; }
    public string? StateJson { get; set; }
    public string? CurrentPhase { get; set; }
    public DateTime? PhaseDeadline { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public List<GamePlayer> Players { get; set; } = new();
}

public class GamePlayer
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string? Role { get; set; }
    public int Score { get; set; }
    public bool IsAlive { get; set; } = true;
    public bool IsConnected { get; set; } = true;
    public DateTime? DisconnectedAt { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public GameSession? Session { get; set; }
    public User? User { get; set; }
}
