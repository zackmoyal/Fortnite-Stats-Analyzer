using FortniteStatsAnalyzer.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace FortniteStatsAnalyzer.Services
{
    public interface IOpenAIService
    {
        Task<string> GenerateStatsFeedback(double kd, double winrate, int topPlacements, int totalKills, int matchesPlayed, string gameMode);
        Task<string> GenerateComprehensiveStatsFeedback(GameMode stats, string gameMode);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;
        private readonly IMemoryCache _cache;  // ADD THIS LINE
        private const string API_URL = "https://api.openai.com/v1/chat/completions";
        private readonly string _apiKey;

        public OpenAIService(IConfiguration configuration, HttpClient httpClient, ILogger<OpenAIService> logger, IMemoryCache cache)  // ADD cache PARAMETER
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));  // ADD THIS LINE

            _apiKey = _configuration["OpenAISettings:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key is not configured");
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured");
            }

            // Set up headers for OpenAI API
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public async Task<string> GenerateStatsFeedback(double kd, double winrate, int topPlacements, int totalKills, int matchesPlayed, string gameMode)
        {
            try
            {
                var systemPrompt = @"You are an expert Fortnite coach providing concise, actionable feedback.
                
                Format your response as exactly 3 sections with these headers:
                🎯 **Performance Analysis**
                💡 **Key Improvements** 
                🚀 **Action Plan**
                
                Keep each section to 2-3 short sentences maximum. Use bullet points for improvements and action items.
                Be direct, encouraging, and use Fortnite terminology (box fights, rotations, zone positioning, etc.).
                Focus on the most impactful advice only.";

                var userPrompt = $@"Analyze these comprehensive Fortnite {gameMode} stats:
                
                Core Performance:
                • K/D Ratio: {kd:F2}
                • Win Rate: {(winrate * 100):F1}%
                • Wins: {topPlacements}
                • Total Kills: {totalKills}
                • Matches Played: {matchesPlayed}
                
                Additional Context:
                • Average performance level based on placement consistency
                • Combat efficiency relative to survival time
                • Strategic positioning and endgame performance
                
                Provide analysis in exactly 3 sections:
                1. Performance Analysis: Brief skill assessment focusing on combat vs survival balance
                2. Key Improvements: 2-3 specific areas (combat skills, positioning, game sense) with bullet points
                3. Action Plan: 2-3 concrete practice steps with bullet points
                
                Keep total response under 200 words. Focus on the most impactful improvements for their skill level.";

                _logger.LogInformation("Generating feedback for {GameMode} stats", gameMode);

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.7,
                    max_tokens = 300
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                _logger.LogDebug("OpenAI Request: {Request}", jsonContent);

                // Create a new request message
                var request = new HttpRequestMessage(HttpMethod.Post, API_URL)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                // Ensure headers are set correctly for this specific request
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("OpenAI Response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    throw new Exception($"OpenAI API error: {response.StatusCode}");
                }

                using JsonDocument doc = JsonDocument.Parse(responseContent);
                var completion = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrEmpty(completion))
                {
                    throw new Exception("Empty feedback received from OpenAI API");
                }

                _logger.LogInformation("Successfully generated feedback for {GameMode}", gameMode);
                return completion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating feedback: {Message}", ex.Message);
                return $"Unable to generate AI feedback at this time. Here's a basic analysis for your {gameMode} stats:\n\n" +
                       $"Your K/D ratio of {kd:F2} and win rate of {(winrate * 100):F1}% show your current performance level. " +
                       $"With {matchesPlayed} matches played and {topPlacements} wins, keep practicing to improve your skills. " +
                       "Focus on building mechanics, positioning, and game awareness to enhance your gameplay.";
            }
        }

        public async Task<string> GenerateComprehensiveStatsFeedback(GameMode stats, string gameMode)
        {
            // Create cache key based on stats that matter
            var cacheKey = $"openai_feedback_{gameMode}_{stats.Kd:F2}_{stats.Winrate:F2}_{stats.PlaceTop1}_{stats.Kills}_{stats.MatchesPlayed}";

            // Try to get from cache
            if (_cache.TryGetValue(cacheKey, out string cachedFeedback))
            {
                _logger.LogInformation("Returning cached AI feedback for {GameMode}", gameMode);
                return cachedFeedback;
            }

            _logger.LogInformation("Generating fresh AI feedback for {GameMode}", gameMode);

            try
            {
                var systemPrompt = @"You are an expert Fortnite coach providing comprehensive, actionable feedback based on detailed player statistics.
        
        Format your response as exactly 3 sections with these headers:
        🎯 **Performance Analysis**
        💡 **Key Improvements** 
        🚀 **Action Plan**
        
        Each section should be 3-4 sentences with detailed analysis. Use bullet points for improvements and action items.
        Be direct, encouraging, and use Fortnite terminology (box fights, rotations, zone positioning, etc.).
        Provide in-depth analysis based on ALL available performance data.";

                // Calculate additional metrics
                var placementConsistency = CalculatePlacementConsistency(stats);
                var combatEfficiency = CalculateCombatEfficiency(stats);
                var survivalSkill = CalculateSurvivalSkill(stats);

                var userPrompt = $@"Analyze these comprehensive Fortnite {gameMode} statistics:

        Core Performance:
        • K/D Ratio: {stats.Kd:F2}
        • Win Rate: {(stats.Winrate * 100):F1}%
        • Wins: {stats.PlaceTop1}
        • Total Kills: {stats.Kills}
        • Matches Played: {stats.MatchesPlayed}

        Placement Consistency:
        • Top 3 Finishes: {stats.Top3 ?? 0} ({(stats.MatchesPlayed > 0 ? (double)(stats.Top3 ?? 0) / stats.MatchesPlayed * 100 : 0):F1}%)
        • Top 5 Finishes: {stats.Top5 ?? 0} ({(stats.MatchesPlayed > 0 ? (double)(stats.Top5 ?? 0) / stats.MatchesPlayed * 100 : 0):F1}%)
        • Top 10 Finishes: {stats.Top10 ?? 0} ({(stats.MatchesPlayed > 0 ? (double)(stats.Top10 ?? 0) / stats.MatchesPlayed * 100 : 0):F1}%)

        Combat & Survival:
        • Deaths: {stats.Deaths ?? 0}
        • Kills Per Minute: {stats.KillsPerMin ?? 0:F2}
        • Minutes Played: {stats.MinutesPlayed ?? 0}
        • Players Outlived: {stats.PlayersOutlived ?? 0}

        Performance Indicators:
        • Placement Consistency: {placementConsistency}
        • Combat Efficiency: {combatEfficiency}
        • Survival Skill: {survivalSkill}

        Provide comprehensive analysis in exactly 3 sections:
        1. Performance Analysis: Detailed assessment of combat vs survival balance, placement consistency, and overall skill level
        2. Key Improvements: 3-4 specific areas focusing on weakest aspects with detailed bullet points
        3. Action Plan: 3-4 concrete practice steps targeting identified weaknesses with specific bullet points

        Keep total response under 500 words. Focus on the most impactful improvements based on their comprehensive performance data.";

                _logger.LogInformation("Generating comprehensive feedback for {GameMode} stats", gameMode);

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
                    temperature = 0.7,
                    max_tokens = 600
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                _logger.LogDebug("OpenAI Comprehensive Request: {Request}", jsonContent);

                var request = new HttpRequestMessage(HttpMethod.Post, API_URL)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("OpenAI Comprehensive Response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    throw new Exception($"OpenAI API error: {response.StatusCode}");
                }

                using JsonDocument doc = JsonDocument.Parse(responseContent);
                var completion = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrEmpty(completion))
                {
                    throw new Exception("Empty feedback received from OpenAI API");
                }

                // Store in cache for 1 hour
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, completion, cacheOptions);
                _logger.LogInformation("Successfully generated and cached comprehensive feedback for {GameMode}", gameMode);

                return completion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comprehensive feedback: {Message}", ex.Message);
                return $"Unable to generate AI feedback at this time. Here's a basic analysis for your {gameMode} stats:\n\n" +
                       $"Your K/D ratio of {stats.Kd:F2} and win rate of {(stats.Winrate * 100):F1}% show your current performance level. " +
                       $"With {stats.MatchesPlayed} matches played and {stats.PlaceTop1} wins, keep practicing to improve your skills. " +
                       "Focus on building mechanics, positioning, and game awareness to enhance your gameplay.";
            }
        }

        private static string CalculatePlacementConsistency(GameMode stats)
        {
            if (stats.MatchesPlayed == 0) return "No Data";
            
            var top10Rate = (double)(stats.Top10 ?? 0) / stats.MatchesPlayed;
            if (top10Rate >= 0.3) return "Excellent";
            if (top10Rate >= 0.2) return "Good";
            if (top10Rate >= 0.1) return "Average";
            return "Needs Work";
        }

        private static string CalculateCombatEfficiency(GameMode stats)
        {
            if (stats.Kd >= 2.0) return "Elite";
            if (stats.Kd >= 1.5) return "Strong";
            if (stats.Kd >= 1.0) return "Solid";
            if (stats.Kd >= 0.5) return "Developing";
            return "Focus Needed";
        }

        private static string CalculateSurvivalSkill(GameMode stats)
        {
            if (stats.MatchesPlayed == 0) return "No Data";
            
            var avgPlacement = (double)(stats.PlayersOutlived ?? 0) / stats.MatchesPlayed;
            if (avgPlacement >= 80) return "Excellent";
            if (avgPlacement >= 60) return "Good";
            if (avgPlacement >= 40) return "Average";
            return "Needs Work";
        }
    }
}

