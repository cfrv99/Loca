using Loca.Domain.Enums;

namespace Loca.Services.Game.Engine;

public interface IGameEngine
{
    GameType GameType { get; }
    object InitializeState(List<Guid> playerIds);
    object ProcessAction(object state, Guid playerId, string action, string? data);
    bool IsGameOver(object state);
    Dictionary<Guid, int> GetScores(object state);
}

public class MafiaEngine : IGameEngine
{
    public GameType GameType => GameType.Mafia;

    public object InitializeState(List<Guid> playerIds)
    {
        var state = new MafiaState
        {
            Phase = MafiaPhase.Day,
            Round = 1,
            AlivePlayers = new List<Guid>(playerIds),
            Roles = AssignRoles(playerIds),
            Votes = new Dictionary<Guid, Guid>()
        };
        return state;
    }

    public object ProcessAction(object state, Guid playerId, string action, string? data)
    {
        var mafiaState = (MafiaState)state;

        switch (action.ToLower())
        {
            case "vote":
                if (data != null && Guid.TryParse(data, out var targetId))
                {
                    mafiaState.Votes[playerId] = targetId;
                    // Check if all alive players voted
                    if (mafiaState.Votes.Count >= mafiaState.AlivePlayers.Count)
                    {
                        ProcessVoteResult(mafiaState);
                    }
                }
                break;

            case "night_action":
                if (mafiaState.Phase == MafiaPhase.Night && data != null)
                {
                    ProcessNightAction(mafiaState, playerId, Guid.Parse(data));
                }
                break;
        }

        return mafiaState;
    }

    public bool IsGameOver(object state)
    {
        var mafiaState = (MafiaState)state;
        var mafiaCount = mafiaState.AlivePlayers.Count(p => mafiaState.Roles[p] == MafiaRole.Mafia);
        var civilianCount = mafiaState.AlivePlayers.Count - mafiaCount;

        return mafiaCount == 0 || mafiaCount >= civilianCount;
    }

    public Dictionary<Guid, int> GetScores(object state)
    {
        var mafiaState = (MafiaState)state;
        var mafiaCount = mafiaState.AlivePlayers.Count(p => mafiaState.Roles[p] == MafiaRole.Mafia);
        var mafiaWins = mafiaCount > 0;

        return mafiaState.Roles.ToDictionary(
            kvp => kvp.Key,
            kvp => (mafiaWins && kvp.Value == MafiaRole.Mafia) ||
                   (!mafiaWins && kvp.Value != MafiaRole.Mafia) ? 100 : 0
        );
    }

    private static Dictionary<Guid, MafiaRole> AssignRoles(List<Guid> playerIds)
    {
        var roles = new Dictionary<Guid, MafiaRole>();
        var shuffled = playerIds.OrderBy(_ => Random.Shared.Next()).ToList();

        var mafiaCount = shuffled.Count switch
        {
            <= 6 => 1,
            <= 9 => 2,
            _ => 3
        };

        for (int i = 0; i < shuffled.Count; i++)
        {
            if (i < mafiaCount)
                roles[shuffled[i]] = MafiaRole.Mafia;
            else if (i == mafiaCount)
                roles[shuffled[i]] = MafiaRole.Doctor;
            else if (i == mafiaCount + 1)
                roles[shuffled[i]] = MafiaRole.Detective;
            else
                roles[shuffled[i]] = MafiaRole.Civilian;
        }

        return roles;
    }

    private static void ProcessVoteResult(MafiaState state)
    {
        var voteCounts = state.Votes.Values.GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .First();

        var eliminatedId = voteCounts.Key;
        state.AlivePlayers.Remove(eliminatedId);
        state.EliminatedPlayers.Add(eliminatedId);
        state.Votes.Clear();
        state.Phase = MafiaPhase.Night;
    }

    private static void ProcessNightAction(MafiaState state, Guid actorId, Guid targetId)
    {
        var role = state.Roles[actorId];
        switch (role)
        {
            case MafiaRole.Mafia:
                state.MafiaTarget = targetId;
                break;
            case MafiaRole.Doctor:
                state.DoctorTarget = targetId;
                break;
            case MafiaRole.Detective:
                state.DetectiveTarget = targetId;
                break;
        }

        // Check if all night actions are done
        var nightActors = state.AlivePlayers.Where(p =>
            state.Roles[p] is MafiaRole.Mafia or MafiaRole.Doctor or MafiaRole.Detective).ToList();

        if (state.MafiaTarget.HasValue &&
            (!state.AlivePlayers.Any(p => state.Roles[p] == MafiaRole.Doctor) || state.DoctorTarget.HasValue) &&
            (!state.AlivePlayers.Any(p => state.Roles[p] == MafiaRole.Detective) || state.DetectiveTarget.HasValue))
        {
            // Process night
            if (state.MafiaTarget != state.DoctorTarget)
            {
                state.AlivePlayers.Remove(state.MafiaTarget.Value);
                state.EliminatedPlayers.Add(state.MafiaTarget.Value);
            }

            state.MafiaTarget = null;
            state.DoctorTarget = null;
            state.DetectiveTarget = null;
            state.Phase = MafiaPhase.Day;
            state.Round++;
        }
    }
}

public class MafiaState
{
    public MafiaPhase Phase { get; set; }
    public int Round { get; set; }
    public List<Guid> AlivePlayers { get; set; } = new();
    public List<Guid> EliminatedPlayers { get; set; } = new();
    public Dictionary<Guid, MafiaRole> Roles { get; set; } = new();
    public Dictionary<Guid, Guid> Votes { get; set; } = new();
    public Guid? MafiaTarget { get; set; }
    public Guid? DoctorTarget { get; set; }
    public Guid? DetectiveTarget { get; set; }
}
