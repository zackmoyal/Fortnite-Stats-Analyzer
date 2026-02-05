// File: Services/FortniteApiConverter.cs
using Newtonsoft.Json.Linq;
using FortniteStatsAnalyzer.Models;
using System.Collections.Generic;
using System.Linq;

namespace FortniteStatsAnalyzer.Services
{
    public static class FortniteApiConverter
    {
        public static FortniteStatsResponse? ConvertFortniteApiResponse(JObject data, string username)
        {
            try
            {
                var response = new FortniteStatsResponse
                {
                    Result = true,
                    Name = username
                };

                // Extract account info
                var account = data["account"] as JObject;
                if (account != null)
                {
                    response.Name = account["name"]?.Value<string>() ?? username;
                }

                // Extract battle pass info for account level history
                var battlePass = data["battlePass"] as JObject;
                if (battlePass != null)
                {
                    var level = battlePass["level"]?.Value<int>() ?? 0;
                    var progress = battlePass["progress"]?.Value<int>() ?? 0;
                    
                    // Create a simple account level history entry
                    response.AccountLevelHistory = new List<AccountLevel>
                    {
                        new AccountLevel
                        {
                            Season = 31, // Current season placeholder
                            Level = level,
                            ProgressPct = progress
                        }
                    };
                }

                // Extract stats
                var stats = data["stats"] as JObject;
                if (stats != null)
                {
                    response.GlobalStats = new GlobalStats();
                    response.PerInput = new PerInput();

                    // Extract all stats (lifetime)
                    var all = stats["all"] as JObject;
                    if (all != null)
                    {
                        // Solo stats
                        var solo = all["solo"] as JObject;
                        if (solo != null)
                        {
                            response.GlobalStats.Solo = ConvertGameModeStats(solo);
                        }

                        // Duo stats  
                        var duo = all["duo"] as JObject;
                        if (duo != null)
                        {
                            response.GlobalStats.Duo = ConvertGameModeStats(duo);
                        }

                        // Squad stats
                        var squad = all["squad"] as JObject;
                        if (squad != null)
                        {
                            response.GlobalStats.Squad = ConvertGameModeStats(squad);
                        }

                        // Trio stats (if available) - Note: not in current model
                        // var trio = all["trio"] as JObject;
                        // if (trio != null)
                        // {
                        //     response.GlobalStats.Trio = ConvertGameModeStats(trio);
                        // }
                    }

                    // Extract input-specific stats
                    var keyboardMouse = stats["keyboardMouse"] as JObject;
                    if (keyboardMouse != null)
                    {
                        response.PerInput.KeyboardMouse = new InputStats();
                        
                        var kmSolo = keyboardMouse["solo"] as JObject;
                        if (kmSolo != null)
                        {
                            response.PerInput.KeyboardMouse.Solo = ConvertGameModeStats(kmSolo);
                        }

                        var kmDuo = keyboardMouse["duo"] as JObject;
                        if (kmDuo != null)
                        {
                            response.PerInput.KeyboardMouse.Duo = ConvertGameModeStats(kmDuo);
                        }

                        var kmSquad = keyboardMouse["squad"] as JObject;
                        if (kmSquad != null)
                        {
                            response.PerInput.KeyboardMouse.Squad = ConvertGameModeStats(kmSquad);
                        }
                    }

                    var gamepad = stats["gamepad"] as JObject;
                    if (gamepad != null)
                    {
                        response.PerInput.Gamepad = new InputStats();
                        
                        var gpSolo = gamepad["solo"] as JObject;
                        if (gpSolo != null)
                        {
                            response.PerInput.Gamepad.Solo = ConvertGameModeStats(gpSolo);
                        }

                        var gpDuo = gamepad["duo"] as JObject;
                        if (gpDuo != null)
                        {
                            response.PerInput.Gamepad.Duo = ConvertGameModeStats(gpDuo);
                        }

                        var gpSquad = gamepad["squad"] as JObject;
                        if (gpSquad != null)
                        {
                            response.PerInput.Gamepad.Squad = ConvertGameModeStats(gpSquad);
                        }
                    }

                    var touch = stats["touch"] as JObject;
                    if (touch != null)
                    {
                        response.PerInput.Touch = new InputStats();
                        
                        var touchSolo = touch["solo"] as JObject;
                        if (touchSolo != null)
                        {
                            response.PerInput.Touch.Solo = ConvertGameModeStats(touchSolo);
                        }

                        var touchDuo = touch["duo"] as JObject;
                        if (touchDuo != null)
                        {
                            response.PerInput.Touch.Duo = ConvertGameModeStats(touchDuo);
                        }

                        var touchSquad = touch["squad"] as JObject;
                        if (touchSquad != null)
                        {
                            response.PerInput.Touch.Squad = ConvertGameModeStats(touchSquad);
                        }
                    }
                }

                return response;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static GameMode ConvertGameModeStats(JObject modeStats)
        {
            return new GameMode
            {
                PlaceTop1 = modeStats["wins"]?.Value<int>() ?? 0,
                Kd = modeStats["kd"]?.Value<double>() ?? 0.0,
                Winrate = modeStats["winRate"]?.Value<double>() ?? 0.0,
                Kills = modeStats["kills"]?.Value<int>() ?? 0,
                MatchesPlayed = modeStats["matches"]?.Value<int>() ?? 0,
                
                // Add all the rich data fields
                Top3 = modeStats["top3"]?.Value<int>(),
                Top5 = modeStats["top5"]?.Value<int>(),
                Top6 = modeStats["top6"]?.Value<int>(),
                Top10 = modeStats["top10"]?.Value<int>(),
                Top12 = modeStats["top12"]?.Value<int>(),
                Top25 = modeStats["top25"]?.Value<int>(),
                Score = modeStats["score"]?.Value<int>(),
                ScorePerMin = modeStats["scorePerMin"]?.Value<double>(),
                ScorePerMatch = modeStats["scorePerMatch"]?.Value<double>(),
                KillsPerMin = modeStats["killsPerMin"]?.Value<double>(),
                KillsPerMatch = modeStats["killsPerMatch"]?.Value<double>(),
                Deaths = modeStats["deaths"]?.Value<int>(),
                MinutesPlayed = modeStats["minutesPlayed"]?.Value<int>(),
                PlayersOutlived = modeStats["playersOutlived"]?.Value<int>(),
                LastModified = modeStats["lastModified"]?.Value<string>()
            };
        }
    }
}

