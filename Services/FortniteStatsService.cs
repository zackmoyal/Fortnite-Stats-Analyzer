using Microsoft.Extensions.Logging;
using System;
using FortniteStatsAnalyzer.Models;
using FortniteStatsAnalyzer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FortniteStatsAnalyzer.Services
{
    public interface IFortniteStatsService
    {
        Task<FortniteStatsResponse?> GetStatsForUser(string username);
        Task<string> GenerateStatsFeedback(double kd, double winrate, int topPlacements, int totalKills, int matchesPlayed, string gameMode);
        Task<string> GenerateComprehensiveStatsFeedback(GameMode stats, string gameMode);
    }

    public class FortniteStatsService : IFortniteStatsService
    {
        private readonly IFortniteApiService _fortniteApiService;
        private readonly IOpenAIService _openAiService;
        private readonly ILogger<FortniteStatsService> _logger;

        public FortniteStatsService(
            IFortniteApiService fortniteApiService,
            IOpenAIService openAiService,
            ILogger<FortniteStatsService> logger)
        {
            _fortniteApiService = fortniteApiService;
            _openAiService = openAiService;
            _logger = logger;
        }

        public async Task<FortniteStatsResponse?> GetStatsForUser(string username)
        {
            return await _fortniteApiService.GetStatsForUser(username);
        }

        public async Task<string> GenerateStatsFeedback(double kd, double winrate, int topPlacements, int totalKills, int matchesPlayed, string gameMode)
        {
            return await _openAiService.GenerateStatsFeedback(kd, winrate, topPlacements, totalKills, matchesPlayed, gameMode);
        }

        public async Task<string> GenerateComprehensiveStatsFeedback(GameMode stats, string gameMode)
        {
            return await _openAiService.GenerateComprehensiveStatsFeedback(stats, gameMode);
        }
    }
}

