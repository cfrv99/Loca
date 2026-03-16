namespace Loca.Application.DTOs;

public record GameSessionDto(
    Guid Id,
    string GameType,
    Guid HostId,
    int MaxPlayers,
    int MinPlayers,
    string Status,
    List<GamePlayerDto> Players
);

public record GamePlayerDto(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    int Score,
    bool IsAlive,
    bool IsConnected
);

public record GameActionDto(
    string ActionType,
    Guid? TargetPlayerId,
    object? Data
);

public record GameResultDto(
    Guid? WinnerId,
    string? WinnerTeam,
    Dictionary<Guid, int> Scores,
    TimeSpan Duration
);

public record TruthOrDareQuestionDto(
    Guid Id,
    string Type,
    string Category,
    string ContentAz,
    string? ContentEn
);

public record QuizQuestionDto(
    Guid Id,
    string Category,
    string QuestionAz,
    List<string> AnswersAz,
    int CorrectIndex
);
