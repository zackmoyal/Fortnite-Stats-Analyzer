namespace FortniteStatsAnalyzer.Configuration
{
    public class FortniteApiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        // Keep configurable, default points to v1
        public string BaseUrl { get; set; } = "https://fortnite-api.com/";
    }
}
