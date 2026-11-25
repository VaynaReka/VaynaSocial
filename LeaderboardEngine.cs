﻿using System.Reflection;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VaynaSocial
{
    public static class LeaderboardEngine
    {
        private static readonly string[] Competitors = new string[]
        {
            "ThunderBear", "ShadowRaven", "SwiftArrow", "IronWolf", "SilentRiver", "StormRider",
            "GhostFox", "EagleFeather", "BraveBison", "NightStalker", "SilverHawk", "RedWillow",
            "LoneHunter", "FireHeart", "HawkEye", "StoneShield", "WildHorse", "WolfClaw",
            "SittingBull", "RedCloud", "CrazyHorse", "Geronimo", "BlackHawk", "RunningDeer"
        };

        private static readonly double[] DecayFactors = new double[]
        {
            1.0, 0.85, 0.72, 0.61, 0.52, 0.44, 0.37, 0.31, 0.26, 0.21,
            0.17, 0.14, 0.11, 0.09, 0.07, 0.05, 0.04, 0.03, 0.025, 0.02,
            0.015, 0.012, 0.009, 0.006
        };

        // Kategori bazli rakip tepe skoru (Benchmark Top Score)
        private static readonly Dictionary<string, double> BenchmarkTopScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            { "TradingTokensGained", 560000.0 }, // Oyuncu 638,098 kurus ile hak ettigi 1. sirada yer alir
            { "TradingToken", 560000.0 },
            { "TradingTokensSpent", 220000.0 },
            { "HumanKills", 750.0 },
            { "HumanKillsHeadshots", 310.0 },
            { "HumanKillsStealth", 38.0 },
            { "HumanStuns", 22.0 },
            { "BrokenBones", 25.0 },
            { "BossKills", 15.0 },
            { "InterrogationsPerformed", 18.0 },
            { "IntimidationsPerformed", 15.0 },
            { "BowshotsHit", 580.0 },
            { "BowshotsFired", 720.0 },
            { "GunshotsHit", 380.0 },
            { "BulletsFired", 520.0 },
            { "FoundersBowKills", 180.0 },
            { "WinchesterKills", 95.0 },
            { "HenryRifleKills", 75.0 },
            { "ColtRevolverKills", 80.0 },
            { "HumanKillsByShotgun", 60.0 },
            { "HumanKillsByBow", 420.0 },
            { "HumanKillsByRifle", 260.0 },
            { "HumanKillsByPistol", 140.0 },
            { "HumanKillsByKnife", 45.0 },
            { "AnimalsKilled", 450.0 },
            { "AnimalKills", 450.0 },
            { "WolvesKilled", 26.0 },
            { "BearsKilled", 22.0 },
            { "BoarsKilled", 18.0 },
            { "AnimalKillsDeers", 35.0 },
            { "AnimalKillsFish", 30.0 },
            { "AnimalKillsRabbits", 25.0 },
            { "SavePointsDiscovered", 16.0 },
            { "CampsCleaned", 32.0 },
            { "SettlementsBurned", 55.0 },
            { "CampsBuilt", 20.0 },
            { "OutpostsCleaned", 12.0 },
            { "EventsTurnedIn", 18.0 },
            { "ItemsCrafted", 280.0 },
            { "SkillsLearned", 35.0 },
            { "TrainsDestroyed", 48.0 } // Oyuncu 51 ile 1. sirada yer alir
        };

        private static Dictionary<string, long> cachedGlobalBinStats;
        private static DateTime lastGlobalBinRead = DateTime.MinValue;

        public static string GetCachedLeaderboardsJson()
        {
            try
            {
                // Her sayac icin oyuncunun gercek skoru ve rakiplerin skorlari hesaplanarak
                // oyuncunun GERCEK siralamasi (PlayerRank: 0, 1, 2, ...) belirlenir.
                var sb = new StringBuilder();
                sb.Append("{\"LeaderboardName\":\"ThisWeek\",\"Counters\":{");

                bool first = true;
                foreach (var kvp in Patch_LeaderboardTab_SetupSelectorAndCounters.CounterDefinitions)
                {
                    string counterId = kvp.Key;
                    int rank = CalculatePlayerRank(counterId);

                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append("\"").Append(counterId).Append("\":{\"PlayerRank\":").Append(rank).Append(",\"NumRanks\":25}");
                }
                sb.Append("}}");

                string result = sb.ToString();
                try
                {
                    string path = Path.Combine(CacheManager.EffectivePluginPath, "cached_leaderboards.json");
                    File.WriteAllText(path, result, new UTF8Encoding(false));
                }
                catch { }

                return result;
            }
            catch (Exception ex)
            {
                VaynaSocialPlugin.Log?.LogWarning("[LeaderboardEngine] GetCachedLeaderboardsJson error: " + ex);
                return CacheManager.GetDefaultLeaderboardsJson();
            }
        }

        public static string GetCachedLeaderboardRangeJson(string counterId)
        {
            if (string.IsNullOrEmpty(counterId)) counterId = "HumanKills";

            try
            {
                bool isPercent = counterId.EndsWith("Accuracy", StringComparison.OrdinalIgnoreCase);
                string playerName = VaynaSocialPlugin.GetPlayerDisplayName();
                if (string.IsNullOrEmpty(playerName)) playerName = "Vayna Reka";

                double playerStat = GetPlayerRealStat(counterId);

                // 24 rakip + 1 oyuncu listesi olustur
                var entries = new List<KeyValuePair<string, double>>(25);
                entries.Add(new KeyValuePair<string, double>(playerName, playerStat));

                double topScore = 100.0;
                if (!BenchmarkTopScores.TryGetValue(counterId, out topScore))
                {
                    topScore = isPercent ? 0.95 : Math.Max(playerStat * 1.25, 50.0);
                }

                for (int i = 0; i < Competitors.Length; i++)
                {
                    double rivalScore;
                    if (isPercent)
                    {
                        rivalScore = Math.Round(Math.Max(0.18, 0.95 - (i * 0.033)), 2);
                    }
                    else
                    {
                        rivalScore = Math.Round(topScore * DecayFactors[i]);
                    }
                    entries.Add(new KeyValuePair<string, double>(Competitors[i], rivalScore));
                }

                // Puana gore buyukten kucuge sirala
                entries.Sort((a, b) => b.Value.CompareTo(a.Value));

                var sb = new StringBuilder();
                sb.Append("{\"LeaderboardName\":\"ThisWeek\",\"CounterId\":\"").Append(counterId)
                  .Append("\",\"FromRank\":0,\"ToRank\":25,\"Range\":{");

                for (int i = 0; i < entries.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append("\"").Append(entries[i].Key).Append("\":");
                    if (isPercent)
                    {
                        sb.Append(entries[i].Value.ToString("0.00", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(entries[i].Value.ToString("0.0", CultureInfo.InvariantCulture));
                    }
                }
                sb.Append("}}");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                VaynaSocialPlugin.Log?.LogWarning("[LeaderboardEngine] GetCachedLeaderboardRangeJson error: " + ex);
                return "{\"LeaderboardName\":\"ThisWeek\",\"CounterId\":\"" + counterId + "\",\"FromRank\":0,\"ToRank\":25,\"Range\":{}}";
            }
        }

        public static int CalculatePlayerRank(string counterId)
        {
            bool isPercent = counterId.EndsWith("Accuracy", StringComparison.OrdinalIgnoreCase);
            double playerStat = GetPlayerRealStat(counterId);

            double topScore = 100.0;
            if (!BenchmarkTopScores.TryGetValue(counterId, out topScore))
            {
                topScore = isPercent ? 0.95 : Math.Max(playerStat * 1.25, 50.0);
            }

            int rank = 0;
            for (int i = 0; i < Competitors.Length; i++)
            {
                double rivalScore = isPercent ? (0.95 - (i * 0.033)) : (topScore * DecayFactors[i]);
                if (rivalScore > playerStat)
                {
                    rank++;
                }
            }
            return Math.Min(rank, 24);
        }

        public static double GetPlayerRealStat(string counterId)
        {
            if (string.IsNullOrEmpty(counterId)) return 0.0;

            // 1. TICARET JETONLARI (Kullanicinin 638,098 Kurusu cached_profile.json'dan anlik okunur)
            if (counterId.IndexOf("Token", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                long tokens = GetProfileTokenCount();
                if (tokens > 0)
                {
                    if (counterId.Equals("TradingTokensSpent", StringComparison.OrdinalIgnoreCase))
                    {
                        return Math.Round(tokens * 0.35); // Harcanan jeton tahmini
                    }
                    return tokens; // 638098
                }
            }

            // 2. YUZDE HESAPLAMALARI (Dogruluk oranlari)
            if (counterId.Equals("BowsAccuracy", StringComparison.OrdinalIgnoreCase))
            {
                double hits = GetStatValue("player:BowshotHit");
                double fired = GetStatValue("player:BowshotFired");
                if (fired > 0) return Math.Round(hits / fired, 2);
                return 0.76;
            }
            if (counterId.Equals("GunsAccuracy", StringComparison.OrdinalIgnoreCase))
            {
                double hits = GetStatValue("player:GunshotHit");
                double fired = GetStatValue("player:GunshotFired");
                if (fired > 0) return Math.Round(hits / fired, 2);
                return 0.86;
            }

            // 3. STAT MAPPING UZERINDEN OYUN VERISI
            long statVal;
            if (TryGetMappedStat(counterId, out statVal) && statVal > 0)
            {
                return statVal;
            }

            // 4. Varsayilan mantikli degerler
            return GetFallbackScore(counterId);
        }

        private static long GetProfileTokenCount()
        {
            try
            {
                string json = CacheManager.GetCachedProfileJson();
                if (!string.IsNullOrEmpty(json))
                {
                    Match m = Regex.Match(json, "\"Prefab\"\\s*:\\s*\"Token Item\"\\s*,\\s*\"Stack\"\\s*:\\s*(\\d+)");
                    if (m.Success)
                    {
                        long stack;
                        if (long.TryParse(m.Groups[1].Value, out stack))
                        {
                            return stack;
                        }
                    }
                }
            }
            catch { }
            return 638098L; // Kullanicinin teyit ettigi kurus miktari
        }

        private static bool TryGetMappedStat(string counterId, out long val)
        {
            val = 0;
            string[] keys;
            if (StatKeyMap.TryGetValue(counterId, out keys))
            {
                foreach (string k in keys)
                {
                    long v = GetStatValue(k);
                    if (v > val) val = v;
                }
            }
            return val > 0;
        }

        private static long GetStatValue(string key)
        {
            // 1. Once aktif oyundaki LocalStatistics.Instance uzerinden sor
            try
            {
                if (LocalStatistics.Instance != null)
                {
                    long v;
                    if (LocalStatistics.Instance.GetStatisticsKey(key, true, out v))
                    {
                        return v;
                    }
                }
            }
            catch { }

            // 2. LocalLow altindaki __Global.bin dosyasindan oku
            EnsureGlobalBinLoaded();
            if (cachedGlobalBinStats != null)
            {
                long v;
                if (cachedGlobalBinStats.TryGetValue(key, out v))
                {
                    return v;
                }
            }
            return 0;
        }

        private static void EnsureGlobalBinLoaded()
        {
            if (cachedGlobalBinStats != null && (DateTime.Now - lastGlobalBinRead).TotalSeconds < 30)
            {
                return;
            }

            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string binPath = Path.Combine(appData, @"..\LocalLow\Game Labs\This Land Is My Land\Statistics\__Global.bin");
                if (File.Exists(binPath))
                {
                    var dummyInst = (LocalStatistics)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(LocalStatistics));
                    var loadMethod = AccessTools.Method(typeof(LocalStatistics), "Load", new Type[] { typeof(string) });
                    if (loadMethod != null)
                    {
                        var dict = loadMethod.Invoke(dummyInst, new object[] { binPath }) as Dictionary<string, long>;
                        if (dict != null)
                        {
                            cachedGlobalBinStats = dict;
                            lastGlobalBinRead = DateTime.Now;
                        }
                    }
                }
            }
            catch { }
        }

        private static double GetFallbackScore(string counterId)
        {
            switch (counterId)
            {
                case "BossKills": return 12;
                case "BrokenBones": return 8;
                case "IntimidationsPerformed": return 6;
                case "FoundersBowKills": return 85;
                case "WinchesterKills": return 42;
                case "HenryRifleKills": return 28;
                case "ColtRevolverKills": return 35;
                case "HumanKillsByShotgun": return 18;
                case "HumanKillsByBow": return 215;
                case "HumanKillsByRifle": return 140;
                case "HumanKillsByPistol": return 65;
                case "HumanKillsByKnife": return 24;
                case "AnimalKillsDeers": return 32;
                case "AnimalKillsRabbits": return 18;
                case "CampsCleaned": return 15;
                case "CampsBuilt": return 9;
                case "EventsTurnedIn": return 7;
                case "SkillsLearned": return 24;
                default: return 10;
            }
        }

        private static readonly Dictionary<string, string[]> StatKeyMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "HumanKills", new[] { "player:KilledHuman", "entity:GroupUnitKilled:Immigrants" } },
            { "HumanKillsHeadshots", new[] { "player:KilledHuman:Headshot" } },
            { "HumanKillsStealth", new[] { "player:KilledHuman:Stealth" } },
            { "HumanStuns", new[] { "player:StunnedHuman", "player:InterrogationResult:Stun" } },
            { "BrokenBones", new[] { "player:BrokenBones", "player:BoneBroken" } },
            { "BossKills", new[] { "player:KilledHuman:Boss", "player:BossKilled" } },
            { "InterrogationsPerformed", new[] { "player:PlayedCutscene:[Cutscene] H-H Interrogate", "player:Interrogation" } },
            { "IntimidationsPerformed", new[] { "player:IntimidationsPerformed", "player:Intimidation" } },
            { "BowshotsHit", new[] { "player:BowshotHit" } },
            { "BowshotsFired", new[] { "player:BowshotFired" } },
            { "GunshotsHit", new[] { "player:GunshotHit" } },
            { "BulletsFired", new[] { "player:GunshotFired", "player:BulletFired" } },
            { "FoundersBowKills", new[] { "player:KilledHuman:Ancestors Bow", "player:BowshotHit:Ancestors Bow" } },
            { "WinchesterKills", new[] { "player:KilledHuman:Winchester", "player:social:GetItem:Winchester shortDLC item" } },
            { "HenryRifleKills", new[] { "player:KilledHuman:Henry", "player:KilledHuman:H.W. Lancaster & Co Model C Rifle" } },
            { "ColtRevolverKills", new[] { "player:KilledHuman:Colt", "player:KilledHuman:Pistol" } },
            { "HumanKillsByShotgun", new[] { "player:KilledHuman:Shotgun", "player:social:GetItem:Sawed off shotgun item" } },
            { "HumanKillsByBow", new[] { "player:KilledHuman:Bow" } },
            { "HumanKillsByRifle", new[] { "player:KilledHuman:Rifle" } },
            { "HumanKillsByPistol", new[] { "player:KilledHuman:Pistol" } },
            { "HumanKillsByKnife", new[] { "player:KilledHuman:Knife", "player:KilledHuman:Stealth:Knife" } },
            { "AnimalsKilled", new[] { "entity:GroupUnitKilled:AnimalZone" } },
            { "AnimalKills", new[] { "entity:GroupUnitKilled:AnimalZone" } },
            { "WolvesKilled", new[] { "player:PickedUpItem:Wolf Pelt" } },
            { "BearsKilled", new[] { "player:PickedUpItem:Bear Pelt" } },
            { "BoarsKilled", new[] { "player:PickedUpItem:Boar Pelt" } },
            { "AnimalKillsDeers", new[] { "player:PickedUpItem:Deer Pelt", "player:PickedUpItem:Buffalo Pelt" } },
            { "AnimalKillsFish", new[] { "player:PickedUpItem:Brook Trout::Fish", "player:PickedUpItem:Red fish meat" } },
            { "AnimalKillsRabbits", new[] { "player:PickedUpItem:Rabbit Pelt" } },
            { "SavePointsDiscovered", new[] { "player:SavePointDiscovered" } },
            { "CampsCleaned", new[] { "player:CampCleaned", "entity:CampMaxUpgraded" } },
            { "SettlementsBurned", new[] { "player:AbilityUsed:Burn Settlement Ability" } },
            { "CampsBuilt", new[] { "player:CampBuilt", "player:PickedUpItem:Long lasting Campfire" } },
            { "OutpostsCleaned", new[] { "player:CampBurnt:Outpost" } },
            { "EventsTurnedIn", new[] { "player:EventsTurnedIn", "player:EventCompleted" } },
            { "ItemsCrafted", new[] { "player:ItemCrafted" } },
            { "SkillsLearned", new[] { "player:SkillsLearned", "player:SkillLearned" } },
            { "TrainsDestroyed", new[] { "train:RailwayEncounterTriggered" } }
        };
    }
}
