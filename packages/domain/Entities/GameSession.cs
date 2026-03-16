using Loca.Domain.Common;
using Loca.Domain.Enums;

namespace Loca.Domain.Entities;

public class GameSession : BaseEntity
{
    public Guid VenueId { get; set; }
    public Guid HostId { get; set; }
    public GameType GameType { get; set; }
    public GameSessionStatus Status { get; set; } = GameSessionStatus.Lobby;
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public string? GameState { get; set; } // JSON serialized state
    public int CurrentRound { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    // Navigation
    public Venue Venue { get; set; } = null!;
    public User Host { get; set; } = null!;
    public List<GamePlayer> Players { get; set; } = new();
}

public class GamePlayer : BaseEntity
{
    public Guid GameSessionId { get; set; }
    public Guid UserId { get; set; }
    public GamePlayerStatus Status { get; set; } = GamePlayerStatus.Waiting;
    public int Score { get; set; }
    public string? RoleData { get; set; } // JSON for role-specific data (e.g., Mafia role)
    public DateTime? DisconnectedAt { get; set; }

    // Navigation
    public GameSession GameSession { get; set; } = null!;
    public User User { get; set; } = null!;
}
