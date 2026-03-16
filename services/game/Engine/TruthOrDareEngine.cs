using Loca.Domain.Enums;

namespace Loca.Services.Game.Engine;

public class TruthOrDareEngine : IGameEngine
{
    public GameType GameType => GameType.TruthOrDare;

    private static readonly List<string> Truths = new()
    {
        "Son 24 saat ərzində etdiyin ən utanc verici şey nə idi?",
        "Telefonda ən çox hansı tətbiqi istifadə edirsən?",
        "Uşaqlıqda ən böyük qorxun nə idi?",
        "Sosial mediada kimin profilini ən çox stalklayırsan?",
        "Axırıncı dəfə nə vaxt ağlamısan və niyə?",
        "Həyatında bir şeyi dəyişə biləsən, nəyi dəyişərdin?",
        "Ən pis xasiyyətin nədir?",
        "Heç birindən utandığın sirr var?",
        "Ən çox kimdən qorxursan?",
        "Əgər görünməz olsaydın, ilk nə edərdin?",
        "Ən böyük yalanın nə olub?",
        "Hansı filmdə yaşamaq istərdin?",
        "Bir gün üçün əks cins olsaydın, nə edərdin?",
        "Rüyalarında ən çox kimi görürsən?",
        "Heç vaxt etiraf etmədiyin şey nədir?",
        "Ən gülməli anın nə olub?",
        "Əgər milyoner olsaydın, ilk nə alardın?",
        "Ən sevdiyin bədən hissən hansıdır?",
        "Heç tanımadığın birinə aşiq olmusan?",
        "Dostların arasında kimi ən çox sevirsən?"
    };

    private static readonly List<string> Dares = new()
    {
        "60 saniyə ərzində ən yaxşı rəqsini göstər!",
        "Yanındakı insana bir iltifat söylə",
        "Telefondakı son mesajı ucadan oxu",
        "30 saniyə gözlərini qırpmadan dur",
        "Bir şəkil çəkdir və sosial mediada paylaş",
        "Bir dəqiqə robot kimi hərəkət et",
        "Ən sevdiyin mahnını oxu",
        "3 nəfərə yad insana salam de",
        "Telefonundakı son selfi göstər",
        "30 saniyə plank tut",
        "Bir nəfərə sarıl",
        "Ən gülməli üz ifadəni göstər",
        "Bir dəqiqə yalnız bəli cavabı ver",
        "Heyvan səsi çıxar",
        "Ən pis rəqsini göstər",
        "Birinin saçını oxşa",
        "Bir şeir söylə",
        "30 saniyə tərsinə danış",
        "Ən sevdiyin yeməyin reseptini anlat",
        "Masaya bir nəğmə həsr et"
    };

    public object InitializeState(List<Guid> playerIds)
    {
        return new TruthOrDareState
        {
            Players = new List<Guid>(playerIds),
            CurrentPlayerIndex = 0,
            Round = 1,
            UsedTruthIndices = new HashSet<int>(),
            UsedDareIndices = new HashSet<int>()
        };
    }

    public object ProcessAction(object state, Guid playerId, string action, string? data)
    {
        var todState = (TruthOrDareState)state;

        if (todState.Players[todState.CurrentPlayerIndex] != playerId)
            return todState;

        switch (action.ToLower())
        {
            case "choose_truth":
                todState.CurrentChallenge = GetRandomItem(Truths, todState.UsedTruthIndices);
                todState.CurrentChoice = "truth";
                break;
            case "choose_dare":
                todState.CurrentChallenge = GetRandomItem(Dares, todState.UsedDareIndices);
                todState.CurrentChoice = "dare";
                break;
            case "complete":
                todState.Scores[playerId] = todState.Scores.GetValueOrDefault(playerId) + 10;
                AdvancePlayer(todState);
                break;
            case "skip":
                AdvancePlayer(todState);
                break;
        }

        return todState;
    }

    public bool IsGameOver(object state)
    {
        var todState = (TruthOrDareState)state;
        return todState.Round > todState.MaxRounds;
    }

    public Dictionary<Guid, int> GetScores(object state)
    {
        var todState = (TruthOrDareState)state;
        return new Dictionary<Guid, int>(todState.Scores);
    }

    private static string GetRandomItem(List<string> items, HashSet<int> usedIndices)
    {
        if (usedIndices.Count >= items.Count)
            usedIndices.Clear();

        int index;
        do
        {
            index = Random.Shared.Next(items.Count);
        } while (usedIndices.Contains(index));

        usedIndices.Add(index);
        return items[index];
    }

    private static void AdvancePlayer(TruthOrDareState state)
    {
        state.CurrentPlayerIndex = (state.CurrentPlayerIndex + 1) % state.Players.Count;
        if (state.CurrentPlayerIndex == 0)
            state.Round++;
        state.CurrentChallenge = null;
        state.CurrentChoice = null;
    }
}

public class TruthOrDareState
{
    public List<Guid> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }
    public int Round { get; set; } = 1;
    public int MaxRounds { get; set; } = 5;
    public string? CurrentChallenge { get; set; }
    public string? CurrentChoice { get; set; }
    public Dictionary<Guid, int> Scores { get; set; } = new();
    public HashSet<int> UsedTruthIndices { get; set; } = new();
    public HashSet<int> UsedDareIndices { get; set; } = new();
}
