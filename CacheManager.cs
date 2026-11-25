using System;

using System.Collections.Generic;

using System.IO;

using System.Text;

using System.Text.RegularExpressions;

using BepInEx;

using UnityEngine;



namespace VaynaSocial

{

	// Token: 0x02000005 RID: 5

	public static class CacheManager

	{

		[Serializable]

		public class CachedProfileDto

		{

			public string PlayerId;

			public string AccountKey;

			public float MaxWeight;

			public int SP;

			public List<CachedItemDto> Items = new List<CachedItemDto>();

			public string AccountType;

			public List<string> KnownWarpaints = new List<string>();

		}



		[Serializable]

		public class CachedItemDto

		{

			public string Prefab;

			public int Stack;

			public bool IsTimeBased;

			public string BundleName;

			public string DlcName;

		}



		public static string EffectivePluginPath

		{

			get

			{

				try

				{

					if (!string.IsNullOrEmpty(Paths.PluginPath))

						return Paths.PluginPath;

				}

				catch {}

				return @"C:\Program Files (x86)\Steam\steamapps\common\This Land Is My Land\BepInEx\plugins";

			}

		}



		public static string CleanJson(string json)

		{

			if (string.IsNullOrEmpty(json)) return json;

			return json.TrimStart('\uFEFF', ' ', '\r', '\n', '\t').TrimEnd();

		}



		// Token: 0x0600001C RID: 28 RVA: 0x00002EE4 File Offset: 0x000010E4

		public static string GetCachedProfileJson()

		{

			if (!string.IsNullOrEmpty(CacheManager.cachedProfileJson))

			{

				return CleanJson(CacheManager.cachedProfileJson);

			}

			try

			{

				string path = Path.Combine(EffectivePluginPath, "cached_profile.json");

				if (File.Exists(path))

				{

					CacheManager.cachedProfileJson = CleanJson(File.ReadAllText(path, Encoding.UTF8));

					return CacheManager.cachedProfileJson;

				}

			}

			catch

			{

			}

			CacheManager.cachedProfileJson = "{\"PlayerId\":\"Vayna Reka\",\"AccountKey\":\"1e97d1ba3314b9ee76c36db6459663c65ddb873f6be458472499eff89073cee6\",\"MaxWeight\":1000000,\"SP\":477600,\"Items\":[{\"Prefab\":\"Token Item\",\"Stack\":638098,\"AdditionalItems\":[]},{\"Prefab\":\"Silver Coin Item\",\"Stack\":171571,\"AdditionalItems\":[]},{\"Prefab\":\"Golden Pocket Watch Item\",\"Stack\":1,\"AdditionalItems\":[]},{\"Prefab\":\"Wrench Item\",\"Stack\":50,\"AdditionalItems\":[]},{\"Prefab\":\"FoundersBow item\",\"Stack\":400,\"AdditionalItems\":[]},{\"Prefab\":\"ItalianWheellock rifle item\",\"Stack\":501,\"AdditionalItems\":[]},{\"Prefab\":\"Bow Arrow Fire\",\"Stack\":3652,\"AdditionalItems\":[]},{\"Prefab\":\"JapaneseMatchlock rifle item\",\"Stack\":417,\"AdditionalItems\":[]},{\"Prefab\":\"Hand Cannon flintlock rifle item\",\"Stack\":1171,\"AdditionalItems\":[]},{\"Prefab\":\"Musketoon flintlock rifle item\",\"Stack\":562,\"AdditionalItems\":[]},{\"Prefab\":\"Assault Gun flintlock pistol item\",\"Stack\":640,\"AdditionalItems\":[]},{\"Prefab\":\"Jail Key Item\",\"Stack\":6503,\"AdditionalItems\":[]},{\"Prefab\":\"BoarPelt Item\",\"Stack\":11436,\"AdditionalItems\":[]},{\"Prefab\":\"Meat Bait Item\",\"Stack\":497,\"AdditionalItems\":[]},{\"Prefab\":\"Grass Bait Item\",\"Stack\":501,\"AdditionalItems\":[]},{\"Prefab\":\"Horse Bait Item\",\"Stack\":7995,\"AdditionalItems\":[]},{\"Prefab\":\"Meat Item\",\"Stack\":2100,\"AdditionalItems\":[]},{\"Prefab\":\"BoarsTusk_item\",\"Stack\":6528,\"AdditionalItems\":[]},{\"Prefab\":\"Rope Item\",\"Stack\":5620,\"AdditionalItems\":[]},{\"Prefab\":\"Wood Item\",\"Stack\":5937,\"AdditionalItems\":[]},{\"Prefab\":\"Native Big World Save Point Item\",\"Stack\":287,\"AdditionalItems\":[]},{\"Prefab\":\"RevolverDLC item\",\"Stack\":3,\"AdditionalItems\":[]},{\"Prefab\":\"Dart Ammo .75\",\"Stack\":600,\"AdditionalItems\":[]},{\"Prefab\":\"Smoked meat item\",\"Stack\":3100,\"AdditionalItems\":[]},{\"Prefab\":\"BuffaloPelt Item\",\"Stack\":6301,\"AdditionalItems\":[]},{\"Prefab\":\"Mortar Grenade 42mm\",\"Stack\":55800,\"AdditionalItems\":[]},{\"Prefab\":\"Stags horn Item\",\"Stack\":19092,\"AdditionalItems\":[]},{\"Prefab\":\"Native World Save Point Item\",\"Stack\":995,\"AdditionalItems\":[]},{\"Prefab\":\"Ball Round .54\",\"Stack\":4000,\"AdditionalItems\":[]},{\"Prefab\":\"India Heal\",\"Stack\":7,\"AdditionalItems\":[]},{\"Prefab\":\"India Health Stamina Boost\",\"Stack\":7,\"AdditionalItems\":[]},{\"Prefab\":\"India Health Stamina Regen\",\"Stack\":30,\"AdditionalItems\":[]},{\"Prefab\":\"Sinew Item\",\"Stack\":7220,\"AdditionalItems\":[]},{\"Prefab\":\"Ammo .41\",\"Stack\":900,\"AdditionalItems\":[]},{\"Prefab\":\"Gauge 16\",\"Stack\":500,\"AdditionalItems\":[]},{\"Prefab\":\"Double barrelled flintlock rifle Optic item\",\"Stack\":261,\"AdditionalItems\":[]},{\"Prefab\":\"Ammo .58\",\"Stack\":10000,\"AdditionalItems\":[]},{\"Prefab\":\"Ball Round .60\",\"Stack\":10000,\"AdditionalItems\":[]},{\"Prefab\":\"Medicine Item\",\"Stack\":10,\"AdditionalItems\":[]},{\"Prefab\":\"Sawed off shotgun item\",\"Stack\":2,\"AdditionalItems\":[]},{\"Prefab\":\"FoundersBow item\",\"Stack\":1,\"IsTimeBased\":true,\"BundleName\":\"Ancestors Bow\",\"DlcName\":\"Founders\",\"AdditionalItems\":[]},{\"Prefab\":\"RevolverDLC item\",\"Stack\":1,\"IsTimeBased\":true,\"BundleName\":\"Feral Spirit Bundle\",\"DlcName\":\"Trophy Weapons\",\"AdditionalItems\":[]},{\"Prefab\":\"Winchester shortDLC item\",\"Stack\":1,\"IsTimeBased\":true,\"BundleName\":\"Claw Bundle\",\"DlcName\":\"Trophy Weapons\",\"AdditionalItems\":[]}],\"AccountType\":\"Standard\",\"NamedEntities\":[],\"MaxNamedEntities\":10,\"KnownWarpaints\":[\"Default\",\"WhiteDLC\",\"BlackRedDLC\",\"Ancestors\",\"Ancestors 2\",\"Skull1BR\"]}";

			return CleanJson(CacheManager.cachedProfileJson);

		}



		// Token: 0x0600001D RID: 29 RVA: 0x00002F58 File Offset: 0x00001158

		public static void SaveCachedProfileJson(string json)

		{

			if (string.IsNullOrEmpty(json) || json.Length < 100)

			{

				return;

			}

			string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();

			json = Regex.Replace(json, "\"PlayerId\"\\s*:\\s*\"[^\"]*\"", "\"PlayerId\":\"" + playerDisplayName + "\"");

			CacheManager.cachedProfileJson = CleanJson(json);

			try

			{

				File.WriteAllText(Path.Combine(EffectivePluginPath, "cached_profile.json"), CacheManager.cachedProfileJson, new UTF8Encoding(false));

			}

			catch

			{

			}

		}



		public static bool DeductItemFromCachedProfile(string prefab, int amount)

		{

			if (string.IsNullOrEmpty(prefab) || amount <= 0) return false;

			try

			{

				string json = GetCachedProfileJson();

				if (string.IsNullOrEmpty(json)) return false;



				string pattern = @"\{[^{}]*""Prefab""\s*:\s*""" + Regex.Escape(prefab) + @"""[^{}]*\}";

				MatchCollection matches = Regex.Matches(json, pattern, RegexOptions.IgnoreCase);

				Match targetMatch = null;

				foreach (Match match in matches)

				{

					if (!match.Value.Contains("\"IsTimeBased\":true"))

					{

						targetMatch = match;

						break;

					}

				}

				if (targetMatch == null && matches.Count > 0)

				{

					targetMatch = matches[0];

				}



				if (targetMatch != null && targetMatch.Success)

				{

					Match stackM = Regex.Match(targetMatch.Value, @"""Stack""\s*:\s*(\d+)");

					if (stackM.Success)

					{

						int curStack = int.Parse(stackM.Groups[1].Value);

						int newStack = curStack - amount;

						string updatedJson;

						if (newStack > 0)

						{

							string newObj = targetMatch.Value.Substring(0, stackM.Groups[1].Index) + newStack + targetMatch.Value.Substring(stackM.Groups[1].Index + stackM.Groups[1].Length);

							updatedJson = json.Substring(0, targetMatch.Index) + newObj + json.Substring(targetMatch.Index + targetMatch.Length);

						}

						else

						{

							int remStart = targetMatch.Index;

							int remLen = targetMatch.Length;

							if (remStart > 0 && json[remStart - 1] == ',')

							{

								remStart--;

								remLen++;

							}

							else if (remStart + remLen < json.Length && json[remStart + remLen] == ',')

							{

								remLen++;

							}

							updatedJson = json.Substring(0, remStart) + json.Substring(remStart + remLen);

						}



						SaveCachedProfileJson(updatedJson);

						return true;

					}

				}

			}

			catch (Exception ex)

			{

				VaynaSocialPlugin.Log?.LogWarning("DeductItemFromCachedProfile error: " + ex.Message);

			}

			return false;

		}



		public static bool AddItemToCachedProfile(string prefab, int amount)

		{

			if (string.IsNullOrEmpty(prefab) || amount <= 0) return false;

			try

			{

				string json = GetCachedProfileJson();

				if (string.IsNullOrEmpty(json)) return false;



				string pattern = @"\{[^{}]*""Prefab""\s*:\s*""" + Regex.Escape(prefab) + @"""[^{}]*\}";

				MatchCollection matches = Regex.Matches(json, pattern, RegexOptions.IgnoreCase);

				Match m = null;

				foreach (Match match in matches)

				{

					if (!match.Value.Contains("\"IsTimeBased\":true"))

					{

						m = match;

						break;

					}

				}

				if (m == null && matches.Count > 0)

				{

					m = matches[0];

				}

				string updatedJson;

				if (m != null && m.Success)

				{

					Match stackM = Regex.Match(m.Value, @"""Stack""\s*:\s*(\d+)");

					if (stackM.Success)

					{

						int curStack = int.Parse(stackM.Groups[1].Value);

						int newStack = curStack + amount;

						string newObj = m.Value.Substring(0, stackM.Groups[1].Index) + newStack + m.Value.Substring(stackM.Groups[1].Index + stackM.Groups[1].Length);

						updatedJson = json.Substring(0, m.Index) + newObj + json.Substring(m.Index + m.Length);

					}

					else

					{

						return false;

					}

				}

				else

				{

					int itemsIdx = json.IndexOf("\"Items\":[", StringComparison.OrdinalIgnoreCase);

					if (itemsIdx >= 0)

					{

						int insertIdx = itemsIdx + "\"Items\":[".Length;

						string newItem = "{\"Prefab\":\"" + prefab + "\",\"Stack\":" + amount + ",\"AdditionalItems\":[]}";

						if (insertIdx < json.Length && json[insertIdx] != ']')

						{

							newItem += ",";

						}

						updatedJson = json.Substring(0, insertIdx) + newItem + json.Substring(insertIdx);

					}

					else

					{

						return false;

					}

				}



				SaveCachedProfileJson(updatedJson);

				return true;

			}

			catch (Exception ex)

			{

				VaynaSocialPlugin.Log?.LogWarning("AddItemToCachedProfile error: " + ex.Message);

			}

			return false;

		}



		public static bool DeductSPFromCachedProfile(int amount)

		{

			if (amount <= 0) return false;

			try

			{

				string json = GetCachedProfileJson();

				if (string.IsNullOrEmpty(json)) return false;



				Match m = Regex.Match(json, @"""SP""\s*:\s*(\d+)");

				if (m.Success)

				{

					int curSp = int.Parse(m.Groups[1].Value);

					int newSp = Math.Max(0, curSp - amount);

					string updatedJson = json.Substring(0, m.Groups[1].Index) + newSp + json.Substring(m.Groups[1].Index + m.Groups[1].Length);

					SaveCachedProfileJson(updatedJson);

					return true;

				}

			}

			catch { return false; }

			return false;

		}



		public static bool AddSPToCachedProfile(int amount)

		{

			if (amount <= 0) return false;

			try

			{

				string json = GetCachedProfileJson();

				Match m = Regex.Match(json, "\"SP\"\\s*:\\s*(\\d+)");

				if (m.Success)

				{

					int curSp = int.Parse(m.Groups[1].Value);

					int newSp = curSp + amount;

					string updatedJson = json.Substring(0, m.Groups[1].Index) + newSp + json.Substring(m.Groups[1].Index + m.Groups[1].Length);

					SaveCachedProfileJson(updatedJson);

					return true;

				}

			}

			catch { }

			return false;

		}



		// Token: 0x0600001E RID: 30 RVA: 0x00002FD0 File Offset: 0x000011D0

		public static string GetCachedEventsJson()

		{

			if (!string.IsNullOrEmpty(CacheManager.cachedEventsJson))

			{

				return CleanJson(CacheManager.cachedEventsJson);

			}

			try

			{

				string path = Path.Combine(EffectivePluginPath, "cached_events.json");

				if (File.Exists(path))

				{

					CacheManager.cachedEventsJson = CleanJson(File.ReadAllText(path, Encoding.UTF8));

					return CacheManager.cachedEventsJson;

				}

				string path2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low", "Game Labs\\This Land Is My Land\\SavedGames\\CachedData_v1s.bin");

				if (File.Exists(path2))

				{

					CacheManager.cachedEventsJson = CleanJson(WorldState.EncryptDecrypt(File.ReadAllText(path2), WorldState.encryptionKey, 0));

					return CacheManager.cachedEventsJson;

				}

			}

			catch

			{

			}

			CacheManager.cachedEventsJson = "{\"ManagerSettings\":{},\"Settings\":[]}";

			return CacheManager.cachedEventsJson;

		}



		// Token: 0x0600001F RID: 31 RVA: 0x00003084 File Offset: 0x00001284

		public static string GetCachedLeaderboardsJson()

		{

			return LeaderboardEngine.GetCachedLeaderboardsJson();

		}



		public static string GetDefaultLeaderboardsJson()

		{

			return "{\"LeaderboardName\":\"ThisWeek\",\"Counters\":{" +

				"\"HumanKills\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"HumanKillsHeadshots\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"HumanKillsStealth\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"HumanStuns\":{\"PlayerRank\":6,\"NumRanks\":25}," +

				"\"BrokenBones\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"BossKills\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"InterrogationsPerformed\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"IntimidationsPerformed\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"BowshotsHit\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"BowshotsFired\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"BowsAccuracy\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"GunshotsHit\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"BulletsFired\":{\"PlayerRank\":6,\"NumRanks\":25}," +

				"\"GunsAccuracy\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"HumanKillsByBow\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"HumanKillsByRifle\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"HumanKillsByPistol\":{\"PlayerRank\":6,\"NumRanks\":25}," +

				"\"HumanKillsByKnife\":{\"PlayerRank\":7,\"NumRanks\":25}," +

				"\"HumanKillsByShotgun\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"AnimalsKilled\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"WolvesKilled\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"BearsKilled\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"BoarsKilled\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"AnimalKillsDeers\":{\"PlayerRank\":6,\"NumRanks\":25}," +

				"\"AnimalKillsFish\":{\"PlayerRank\":7,\"NumRanks\":25}," +

				"\"AnimalKillsRabbits\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"SavePointsDiscovered\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"CampsCleaned\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"SettlementsBurned\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"CampsBuilt\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"OutpostsCleaned\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"TradingTokensGained\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"TradingTokensSpent\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"EventsTurnedIn\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"ItemsCrafted\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"SkillsLearned\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"TrainsDestroyed\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"FoundersBowKills\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"WinchesterKills\":{\"PlayerRank\":4,\"NumRanks\":25}," +

				"\"HenryRifleKills\":{\"PlayerRank\":5,\"NumRanks\":25}," +

				"\"ColtRevolverKills\":{\"PlayerRank\":4,\"NumRanks\":25}" +

				"}}";

		}



		// Token: 0x06000020 RID: 32 RVA: 0x000030EC File Offset: 0x000012EC

		public static string GetCachedAuctionJson()

		{

			if (!string.IsNullOrEmpty(CacheManager.cachedAuctionJson))

			{

				return CleanJson(CacheManager.cachedAuctionJson);

			}

			try

			{

				string path = Path.Combine(EffectivePluginPath, "cached_auction.json");

				if (File.Exists(path))

				{

					CacheManager.cachedAuctionJson = CleanJson(File.ReadAllText(path, Encoding.UTF8));

					return CacheManager.cachedAuctionJson;

				}

			}

			catch

			{

			}

			return "[]";

		}



		public static string GetCachedPlacedItemsJson()

		{

			try

			{

				string path = Path.Combine(EffectivePluginPath, "cached_placed_items.json");

				if (File.Exists(path))

				{

					return CleanJson(File.ReadAllText(path, Encoding.UTF8));

				}

			}

			catch {}

			return "[]";

		}



		public static void SaveCachedPlacedItemsJson(string json)

		{

			try

			{

				string path = Path.Combine(EffectivePluginPath, "cached_placed_items.json");

				File.WriteAllText(path, CleanJson(json), new UTF8Encoding(false));

			}

			catch {}

		}



		// Token: 0x06000021 RID: 33 RVA: 0x00003154 File Offset: 0x00001354

		public static string GetCachedLeaderboardRangeJson(string counterId)

		{

			return LeaderboardEngine.GetCachedLeaderboardRangeJson(counterId);

		}



		// Token: 0x04000019 RID: 25

		private static string cachedProfileJson;



		// Token: 0x0400001A RID: 26

		private static string cachedEventsJson;



		// Token: 0x0400001B RID: 27

		



		// Token: 0x0400001C RID: 28

		private static string cachedAuctionJson;

	}

}