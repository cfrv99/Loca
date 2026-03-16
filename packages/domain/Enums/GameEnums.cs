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

public enum GameStatus
{
    Lobby,
    InProgress,
    Completed,
    Cancelled
}

public enum MafiaRole
{
    Mafia,
    Doctor,
    Detective,
    Citizen
}

public enum MafiaPhase
{
    Night,
    DayDiscussion,
    DayVote,
    Elimination
}

public enum TruthOrDareChoice
{
    Truth,
    Dare
}
