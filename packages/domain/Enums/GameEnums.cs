namespace Loca.Domain.Enums;

public enum GameType
{
    Mafia,
    TruthOrDare,
    Uno,
    Domino,
    QuizBattle,
    WouldYouRather
}

public enum GameSessionStatus
{
    Lobby,
    InProgress,
    Paused,
    Completed,
    Cancelled
}

public enum MafiaRole
{
    Civilian,
    Mafia,
    Doctor,
    Detective
}

public enum MafiaPhase
{
    Day,
    Night,
    Voting,
    Result
}

public enum GamePlayerStatus
{
    Waiting,
    Ready,
    Playing,
    Eliminated,
    Disconnected
}
