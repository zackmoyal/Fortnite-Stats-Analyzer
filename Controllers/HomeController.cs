// File: Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FortniteStatsAnalyzer.Services;
using FortniteStatsAnalyzer.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FortniteStatsAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFortniteStatsService _fortniteStatsService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFortniteStatsService fortniteStatsService, ILogger<HomeController> logger)
        {
            _fortniteStatsService = fortniteStatsService;
            _logger = logger;
        }

        // GET: /
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/ValidateUsername?username=...
        [HttpGet]
        public async Task<IActionResult> ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Validation failed: No username provided.");
                return Json(new { success = false, message = "Please enter a Fortnite username." });
            }

            var stats = await _fortniteStatsService.GetStatsForUser(username);

            if (stats is null)
                return Json(new { success = false, message = "Unable to reach stats service." });

            if (string.Equals(stats.Error, "Invalid account", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "Invalid username. Please try again." });

            // Allow continue even if stats are empty; we'll show placeholders in the Stats view.
            return Json(new { success = true });
        }

        // POST: /Home/GetStats
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetStats(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("No username provided by the user.");
                ViewBag.Error = "Please enter a Fortnite username.";
                return View("Index");
            }

            var stats = await _fortniteStatsService.GetStatsForUser(username);

            if (stats is null)
            {
                _logger.LogWarning("Stats service returned null for user: {Username}", username);
                ViewBag.Error = "We couldn't reach the stats service. Please try again.";
                return View("Index");
            }

            if (!stats.Result)
            {
                if (string.Equals(stats.Error, "Invalid account", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid account for user: {Username}", username);
                    ViewBag.Error = "Invalid username. Please try again.";
                }
                else
                {
                    _logger.LogWarning("No stats available for user: {Username}. Error: {Error}", username, stats.Error);
                    ViewBag.Error = stats.Error ?? "No player stats available. Please check the username and try again.";
                }
                return View("Index");
            }

            var hasStats = HasAnyStats(stats);
            ViewBag.HasStats = hasStats; // used by view to decide between table vs. placeholders

            _logger.LogInformation("Stats retrieved for {Username}: Result={Result}, Name={Name}, HasStats={HasStats}",
                username, stats.Result, stats.Name, hasStats);

            if (stats.GlobalStats != null)
            {
                _logger.LogInformation("GlobalStats: Solo={HasSolo}, Duo={HasDuo}, Squad={HasSquad}",
                    stats.GlobalStats.Solo != null,
                    stats.GlobalStats.Duo != null,
                    stats.GlobalStats.Squad != null);
            }
            else
            {
                _logger.LogWarning("GlobalStats is null for user {Username}", username);
            }

            // Always render the Stats view on Result==true, even when no BR modes are present.
            return View("StatsView", stats);
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // Default error handler
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Mirrors the service-side heuristic; keeps controller logic self-contained for the view.
        private static bool HasAnyStats(FortniteStatsResponse? s)
        {
            if (s is null) return false;
            var g = s.GlobalStats;
            var p = s.PerInput;
            return (g?.Solo != null) || (g?.Duo != null) || (g?.Squad != null) ||
                   (p?.Gamepad?.Solo != null) || (p?.Gamepad?.Duo != null) || (p?.Gamepad?.Squad != null) ||
                   (p?.KeyboardMouse?.Solo != null) || (p?.KeyboardMouse?.Duo != null) || (p?.KeyboardMouse?.Squad != null);
        }
    }
}
