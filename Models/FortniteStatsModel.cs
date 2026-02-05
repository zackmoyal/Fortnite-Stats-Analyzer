namespace FortniteStatsAnalyzer.Models
{
    public class FortniteStatsModel
    {
        public required string Username { get; set; }
        public int Kills { get; set; }
        public int Wins {  get; set; }
        public double KdRatio { get; set; }
        public int MatchesPlayed { get; set; }
    }
}
