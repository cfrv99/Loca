namespace Loca.Application.DTOs;

public record GameSessionDto(
    Guid Id,
    string GameType,
    string Status,
    Guid HostId,
    string HostName,
    int PlayerCount,
    int MinPlayers,
    int MaxPlayers,
    DateTime CreatedAt
);

public record GameLobbyDto(
    Guid SessionId,
    string GameType,
    string Status,
    List<GamePlayerDto> Players,
    Guid HostId,
    int MinPlayers,
    int MaxPlayers
);

public record GamePlayerDto(
    Guid UserId,
    string DisplayName,
    string? ProfilePhotoUrl,
    string Status,
    int Score
);

public record GameStateDto(
    Guid SessionId,
    string GameType,
    int CurrentRound,
    string Phase,
    object? StateData // Game-specific state
);
