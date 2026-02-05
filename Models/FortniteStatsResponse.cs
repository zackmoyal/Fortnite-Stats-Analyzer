using Newtonsoft.Json;

namespace FortniteStatsAnalyzer.Models
{
    public class FortniteStatsResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("data")]
        public FortniteData? Data { get; set; }

        // Legacy properties for backward compatibility - these will be populated by the controller
        public bool Result { get; set; }
        public string? Error { get; set; }
        public string? Name { get; set; }
        public string? AccountId { get; set; }
        public Account? Account { get; set; }
        public List<AccountLevel>? AccountLevelHistory { get; set; }
        public GlobalStats? GlobalStats { get; set; }
        public PerInput? PerInput { get; set; }
        public List<int>? SeasonsAvailable { get; set; }
        public int? SeasonUsed { get; set; }
    }

    public class FortniteData
    {
        [JsonProperty("account")]
        public AccountInfo? Account { get; set; }

        [JsonProperty("battlePass")]
        public BattlePass? BattlePass { get; set; }

        [JsonProperty("stats")]
        public StatsContainer? Stats { get; set; }
    }

    public class AccountInfo
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class BattlePass
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }
    }

    public class StatsContainer
    {
        [JsonProperty("all")]
        public AllStats? All { get; set; }

        [JsonProperty("keyboardMouse")]
        public InputStats? KeyboardMouse { get; set; }

        [JsonProperty("gamepad")]
        public InputStats? Gamepad { get; set; }

        [JsonProperty("touch")]
        public InputStats? Touch { get; set; }
    }

    public class AllStats
    {
        [JsonProperty("overall")]
        public GameMode? Overall { get; set; }

        [JsonProperty("solo")]
        public GameMode? Solo { get; set; }

        [JsonProperty("duo")]
        public GameMode? Duo { get; set; }

        [JsonProperty("squad")]
        public GameMode? Squad { get; set; }

        [JsonProperty("ltm")]
        public GameMode? Ltm { get; set; }
    }

    public class GlobalStats
    {
        public GameMode? Solo { get; set; }
        public GameMode? Duo { get; set; }
        public GameMode? Squad { get; set; }
    }

    public class GameMode
    {
        [JsonProperty("wins")]
        public int PlaceTop1 { get; set; }

        [JsonProperty("kd")]
        public double Kd { get; set; }

        [JsonProperty("winRate")]
        public double Winrate { get; set; }

        [JsonProperty("kills")]
        public int Kills { get; set; }

        [JsonProperty("matches")]
        public int MatchesPlayed { get; set; }

        [JsonProperty("top3")]
        public int? Top3 { get; set; }

        [JsonProperty("top5")]
        public int? Top5 { get; set; }

        [JsonProperty("top6")]
        public int? Top6 { get; set; }

        [JsonProperty("top10")]
        public int? Top10 { get; set; }

        [JsonProperty("top12")]
        public int? Top12 { get; set; }

        [JsonProperty("top25")]
        public int? Top25 { get; set; }

        [JsonProperty("score")]
        public int? Score { get; set; }

        [JsonProperty("scorePerMin")]
        public double? ScorePerMin { get; set; }

        [JsonProperty("scorePerMatch")]
        public double? ScorePerMatch { get; set; }

        [JsonProperty("killsPerMin")]
        public double? KillsPerMin { get; set; }

        [JsonProperty("killsPerMatch")]
        public double? KillsPerMatch { get; set; }

        [JsonProperty("deaths")]
        public int? Deaths { get; set; }

        [JsonProperty("minutesPlayed")]
        public int? MinutesPlayed { get; set; }

        [JsonProperty("playersOutlived")]
        public int? PlayersOutlived { get; set; }

        [JsonProperty("lastModified")]
        public string? LastModified { get; set; }
    }

    public class PerInput
    {
        public InputStats? Gamepad { get; set; }
        public InputStats? KeyboardMouse { get; set; }
        public InputStats? Touch { get; set; }
    }

    public class InputStats
    {
        [JsonProperty("overall")]
        public GameMode? Overall { get; set; }

        [JsonProperty("solo")]
        public GameMode? Solo { get; set; }

        [JsonProperty("duo")]
        public GameMode? Duo { get; set; }

        [JsonProperty("squad")]
        public GameMode? Squad { get; set; }

        [JsonProperty("ltm")]
        public GameMode? Ltm { get; set; }
    }

    public class Account
    {
        public int Level { get; set; }
        public int ProgressPct { get; set; }
    }

    public class AccountLevel
    {
        public int Season { get; set; }
        public int Level { get; set; }
        public int ProgressPct { get; set; }
    }
}

