// File: Services/FortniteApiService.cs
using FortniteStatsAnalyzer.Configuration;
using FortniteStatsAnalyzer.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace FortniteStatsAnalyzer.Services
{
    public interface IFortniteApiService
    {
        Task<FortniteStatsResponse?> GetStatsForUser(string username);
    }

    public class FortniteApiService : IFortniteApiService
    {
        private readonly HttpClient _client;
        private readonly string _fortniteApiKey;
        private readonly ILogger<FortniteApiService> _logger;

        // NOTE: BaseAddress, Accept, and Authorization are configured in Program.cs
        private readonly IMemoryCache _cache;

        public FortniteApiService(
            IOptions<FortniteApiSettings> fortniteSettings,
            ILogger<FortniteApiService> logger,
            HttpClient httpClient,
            IMemoryCache cache)
        {
            _fortniteApiKey = fortniteSettings?.Value?.ApiKey?.Trim()
                ?? throw new InvalidOperationException("Fortnite API key is not set correctly in configuration.");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<FortniteStatsResponse?> GetStatsForUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Empty username provided");
                return new FortniteStatsResponse { Result = false, Error = "Username cannot be empty" };
            }

            var normalizedUsername = username.Trim();
            _logger.LogInformation("Getting stats for username: {Username}", normalizedUsername);

            // Try exact username
            var stats = await TryGetStats(normalizedUsername);

            if (stats?.Result == true)
            {
                if (HasAnyStats(stats))
                {
                    _logger.LogInformation("Found stats with exact username match");
                    return stats;
                }

                // Try account lookup if lifetime empty
                _logger.LogInformation("Username resolved but stats empty; trying account lookup fallback.");
                var alt = await TryGetStatsByAccountId(normalizedUsername);
                if (HasAnyStats(alt))
                    return alt;

                // Return the stats even if empty (account exists)
                return stats;
            }

            // If API says invalid account, try lookup
            if (stats?.Error == "Invalid account")
            {
                _logger.LogInformation("Trying account lookup for username");
                var alt = await TryGetStatsByAccountId(normalizedUsername);
                if (alt?.Result == true)
                    return alt;
            }

            return new FortniteStatsResponse
            {
                Result = false,
                Error = "No player stats available. Please check the username and try again."
            };
        }

        /// <summary>
        /// Try by username. If lifetime empty, automatically try latest available season for that response.
        /// </summary>
        private async Task<FortniteStatsResponse?> TryGetStats(string username)
        {
            // Create cache key (lowercase for consistency)
            var cacheKey = $"fortnite_stats_{username.ToLower()}";

            // Try to get from cache
            if (_cache.TryGetValue(cacheKey, out FortniteStatsResponse cachedStats))
            {
                _logger.LogInformation("Returning cached stats for {Username}", username);
                return cachedStats;
            }

            _logger.LogInformation("Fetching fresh stats for {Username} from API", username);

            // YOUR EXISTING API CALL CODE GOES HERE (Don't change this part, just wrap it)
            try
            {
                var requestUrl = $"v2/stats/br/v2?name={Uri.EscapeDataString(username)}";
                _logger.LogDebug("Making request to: {Url}", requestUrl);

                var response = await _client.GetAsync(requestUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Raw API response for {Username}: {Content}", username, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 429)
                    {
                        _logger.LogWarning("Rate limit hit, waiting before retry");
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        return null; // let caller retry on next loop
                    }
                    _logger.LogWarning("Non-success status code {StatusCode} for username {Username}", (int)response.StatusCode, username);
                    return null;
                }

                // Parse Fortnite-API.com response format
                var apiResponse = JsonConvert.DeserializeObject<JObject>(responseContent);
                if (apiResponse == null)
                {
                    _logger.LogWarning("Failed to parse API response for {Username}", username);
                    return null;
                }

                // Check if the API call was successful
                var status = apiResponse["status"]?.Value<int>();
                if (status != 200)
                {
                    var error = apiResponse["error"]?.Value<string>() ?? "Unknown error";
                    _logger.LogWarning("API returned error status {Status}: {Error} for username {Username}", status, error, username);
                    return new FortniteStatsResponse { Result = false, Error = error };
                }

                // Extract data from the response
                var data = apiResponse["data"] as JObject;
                if (data == null)
                {
                    _logger.LogWarning("No data section in API response for {Username}", username);
                    return new FortniteStatsResponse { Result = false, Error = "No data available" };
                }

                // Convert Fortnite-API.com format to our model format
                var stats = FortniteApiConverter.ConvertFortniteApiResponse(data, username);

                if (stats != null)
                {
                    _logger.LogInformation(
                        "Converted stats - Result: {Result}, Name: {Name}, GlobalStats null: {GlobalStatsNull}",
                        stats.Result, stats.Name, stats.GlobalStats == null);

                    if (stats.GlobalStats != null)
                    {
                        _logger.LogInformation(
                            "GlobalStats properties - Solo: {Solo}, Duo: {Duo}, Squad: {Squad}",
                            stats.GlobalStats.Solo != null,
                            stats.GlobalStats.Duo != null,
                            stats.GlobalStats.Squad != null);
                    }

                    // Store in cache for 5 minutes (only if successful)
                    if (stats.Result)
                    {
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                            .SetPriority(CacheItemPriority.Normal);

                        _cache.Set(cacheKey, stats, cacheOptions);
                        _logger.LogInformation("Cached stats for {Username}", username);
                    }
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for username {Username}", username);
                return null;
            }
        }

        /// <summary>
        /// Simplified method - Fortnite-API.com doesn't need complex fallbacks
        /// </summary>
        private async Task<FortniteStatsResponse?> TryGetStatsByAccountId(string username)
        {
            // Fortnite-API.com uses name-based lookup, so just try the same endpoint
            return await TryGetStats(username);
        }

        /// <summary>
        /// Fortnite-API.com doesn't support seasonal queries in the same way
        /// </summary>
        private Task<FortniteStatsResponse?> TryGetStatsSeasonByUsername(string username, int season)
        {
            // For now, just return null - Fortnite-API.com uses different seasonal approach
            _logger.LogInformation("Seasonal queries not implemented for Fortnite-API.com");
            return Task.FromResult<FortniteStatsResponse?>(null);
        }

        /// <summary>
        /// Fortnite-API.com doesn't support seasonal queries by account ID
        /// </summary>
        private Task<FortniteStatsResponse?> TryGetStatsSeasonByAccount(string accountId, int season)
        {
            // For now, just return null - Fortnite-API.com uses different seasonal approach
            _logger.LogInformation("Seasonal queries by account not implemented for Fortnite-API.com");
            return Task.FromResult<FortniteStatsResponse?>(null);
        }

        /// <summary>
        /// Returns true if any BR mode exists across lifetime or per-input with actual data.
        /// </summary>
        private static bool HasAnyStats(FortniteStatsResponse? s)
        {
            if (s is null) return false;
            
            // Check global stats for any meaningful data
            var g = s.GlobalStats;
            if (g != null)
            {
                if (HasMeaningfulGameModeStats(g.Solo) || 
                    HasMeaningfulGameModeStats(g.Duo) || 
                    HasMeaningfulGameModeStats(g.Squad))
                {
                    return true;
                }
            }

            // Check per-input stats for any meaningful data
            var p = s.PerInput;
            if (p != null)
            {
                if (p.Gamepad != null && (HasMeaningfulGameModeStats(p.Gamepad.Solo) || 
                                         HasMeaningfulGameModeStats(p.Gamepad.Duo) || 
                                         HasMeaningfulGameModeStats(p.Gamepad.Squad)))
                {
                    return true;
                }
                
                if (p.KeyboardMouse != null && (HasMeaningfulGameModeStats(p.KeyboardMouse.Solo) || 
                                               HasMeaningfulGameModeStats(p.KeyboardMouse.Duo) || 
                                               HasMeaningfulGameModeStats(p.KeyboardMouse.Squad)))
                {
                    return true;
                }
                
                if (p.Touch != null && (HasMeaningfulGameModeStats(p.Touch.Solo) || 
                                       HasMeaningfulGameModeStats(p.Touch.Duo) || 
                                       HasMeaningfulGameModeStats(p.Touch.Squad)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method to check if a GameMode has meaningful stats (matches played > 0)
        /// </summary>
        private static bool HasMeaningfulGameModeStats(GameMode? mode)
        {
            return mode != null && mode.MatchesPlayed > 0;
        }

        private static string[] GenerateCaseVariants(string s)
        {
            if (string.IsNullOrEmpty(s)) return Array.Empty<string>();

            var title = s.Length == 1
                ? s.ToUpperInvariant()
                : $"{char.ToUpperInvariant(s[0])}{s[1..].ToLowerInvariant()}";

            return new[]
            {
                s.ToLowerInvariant(),
                s.ToUpperInvariant(),
                title
            };
        }

        /// <summary>
        /// Optionally stamp a SeasonUsed value on the model if you added
        ///   public int? SeasonUsed { get; set; }
        /// to FortniteStatsResponse. If not present, this does nothing.
        /// </summary>
        private void TryStampSeasonUsed(FortniteStatsResponse? stats, int season)
        {
            if (stats == null) return;

            var prop = typeof(FortniteStatsResponse).GetProperty("SeasonUsed");
            if (prop != null && prop.PropertyType == typeof(int?))
            {
                try { prop.SetValue(stats, season); } catch { /* ignore */ }
            }
        }
    }
}
