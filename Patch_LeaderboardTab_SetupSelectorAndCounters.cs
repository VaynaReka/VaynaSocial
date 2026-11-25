using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UIWidgets;
using UnityEngine.Events;

namespace VaynaSocial
{
	public struct CounterMeta
	{
		public string CategoryKey;
		public string TrName;
		public string EnName;
		public bool IsPercent;

		public CounterMeta(string catKey, string trName, string enName, bool isPercent = false)
		{
			CategoryKey = catKey;
			TrName = trName;
			EnName = enName;
			IsPercent = isPercent;
		}
	}

	[HarmonyPatch]
	internal static class Patch_LeaderboardTab_SetupSelectorAndCounters
	{
		private static MethodBase TargetMethod()
		{
			return AccessTools.Method(typeof(LeaderboardTab), "SetupSelectorAndCounters");
		}

		public static readonly (string Key, string Tr, string En)[] CategoryOrder = new (string, string, string)[]
		{
			("Hunting", "Avlanma", "Hunting"),
			("EnemyKills", "Düşman Öldürmeleri", "Enemy Kills"),
			("Weapons", "Silahlar", "Weapons"),
			("Misc", "Karışık", "Miscellaneous"),
			("RealisticDamage", "Gerçekçi Hasar", "Realistic Damage"),
			("WeaponTypeKills", "Silah Türüne Göre Düşman Öldürmeleri", "Kills by Weapon Type"),
			("IndividualWeapons", "Bireysel Silahlarla Düşman Öldürmeleri", "Individual Weapon Kills"),
			("Stealth", "Gizlilik", "Stealth"),
			("EnemyTypes", "Düşman Çeşidi", "Enemy Types"),
			("Settlements", "Yerleşkeler", "Settlements"),
			("Trading", "Sağlık & Ticaret", "Trading & Wealth"),
			("Trains", "Trenler", "Trains")
		};

		public static readonly Dictionary<string, CounterMeta> CounterDefinitions = new Dictionary<string, CounterMeta>(StringComparer.OrdinalIgnoreCase)
		{
			// Avlanma / Hunting
			{ "AnimalsKilled", new CounterMeta("Hunting", "Toplam Hayvan Öldürmeleri", "Total Animals Killed") },
			{ "Animals Killed", new CounterMeta("Hunting", "Toplam Hayvan Öldürmeleri", "Total Animals Killed") },
			{ "AnimalKills", new CounterMeta("Hunting", "Toplam Hayvan Öldürmeleri", "Total Animals Killed") },
			{ "AnimalKillsDeers", new CounterMeta("Hunting", "Öldürülen Geyikler", "Deers Killed") },
			{ "AnimalKillsFish", new CounterMeta("Hunting", "Öldürülen Balıklar", "Fish Caught") },
			{ "AnimalKillsRabbits", new CounterMeta("Hunting", "Öldürülen Tavşanlar", "Rabbits Killed") },
			{ "BearsKilled", new CounterMeta("Hunting", "Öldürülen Ayılar", "Bears Killed") },
			{ "BoarsKilled", new CounterMeta("Hunting", "Öldürülen Domuzlar", "Boars Killed") },
			{ "WolvesKilled", new CounterMeta("Hunting", "Öldürülen Kurtlar", "Wolves Killed") },

			// Düşman Öldürmeleri / Enemy Kills
			{ "BossKills", new CounterMeta("EnemyKills", "Boss Öldürmeleri", "Bosses Killed") },
			{ "HumanKills", new CounterMeta("EnemyKills", "İnsan Öldürmeleri", "Human Kills") },
			{ "HumanKillsHeadshots", new CounterMeta("EnemyKills", "Kafadan Vuruşlar", "Headshots") },

			// Silahlar / Weapons
			{ "BowsAccuracy", new CounterMeta("Weapons", "Yayların Doğruluğu", "Bow Accuracy", true) },
			{ "BowshotsFired", new CounterMeta("Weapons", "Atılan Oklar", "Arrows Fired") },
			{ "BowshotsHit", new CounterMeta("Weapons", "İsabet Alan Oklar", "Arrows Hit") },
			{ "BulletsFired", new CounterMeta("Weapons", "Sıkılan Mermiler", "Bullets Fired") },
			{ "GunsAccuracy", new CounterMeta("Weapons", "Silahların Doğruluğu", "Gun Accuracy", true) },
			{ "GunshotsFired", new CounterMeta("Weapons", "Yapılan Atışlar", "Shots Fired") },
			{ "GunshotsHit", new CounterMeta("Weapons", "İsabet Alan Atışlar", "Shots Hit") },

			// Karışık / Misc
			{ "EventsTurnedIn", new CounterMeta("Misc", "Tamamlanan Etkinlikler", "Events Completed") },
			{ "Leaderboards_Counter_EventsTurnedIn", new CounterMeta("Misc", "Tamamlanan Etkinlikler", "Events Completed") },
			{ "ItemsCrafted", new CounterMeta("Misc", "Üretilen Öğeler", "Items Crafted") },
			{ "SkillsLearned", new CounterMeta("Misc", "Öğrenilen Beceriler", "Skills Learned") },

			// Gerçekçi Hasar / Realistic Damage
			{ "FractureCaused", new CounterMeta("RealisticDamage", "Kırık Oldu", "Fractures Caused") },
			{ "BrokenBones", new CounterMeta("RealisticDamage", "Kırık Oldu", "Broken Bones") },
			{ "HeartWound", new CounterMeta("RealisticDamage", "Kalbinden Yaralanan İnsanlar", "Heart Wounds") },
			{ "HeavyBleedCaused", new CounterMeta("RealisticDamage", "Ağır Kanama Oldu", "Heavy Bleeds Caused") },
			{ "LightBleedCaused", new CounterMeta("RealisticDamage", "Hafif Kanama Oldu", "Light Bleeds Caused") },

			// Silah Türüne Göre Düşman Öldürmeleri / Weapon Type Kills
			{ "HumanKillsByBow", new CounterMeta("WeaponTypeKills", "Yay ile Yapılan Öldürmeler", "Kills by Bow") },
			{ "HumanKillsByKnife", new CounterMeta("WeaponTypeKills", "Bıçakla Yapılan Öldürmeler", "Kills by Knife") },
			{ "HumanKillsByPistol", new CounterMeta("WeaponTypeKills", "Tabanca ile Öldürmeler", "Kills by Pistol") },
			{ "HumanKillsByRifle", new CounterMeta("WeaponTypeKills", "Tüfek ile Öldürmeler", "Kills by Rifle") },
			{ "HumanKillsHeadshotsByBow", new CounterMeta("WeaponTypeKills", "Yay ile Kafadan Vuruşlar", "Headshots by Bow") },
			{ "HumanKillsHeadshotsByPistol", new CounterMeta("WeaponTypeKills", "Tabanca ile Kafadan Vuruşlar", "Headshots by Pistol") },
			{ "HumanKillsHeadshotsByRifle", new CounterMeta("WeaponTypeKills", "Tüfekle Kafadan Vuruşlar", "Headshots by Rifle") },

			// Gizlilik / Stealth
			{ "HumanKillsStealth", new CounterMeta("Stealth", "Gizli İnsan Öldürmeleri", "Stealth Human Kills") },
			{ "InterrogationResultsPerformed", new CounterMeta("Stealth", "Başarılı Sorgulamalar", "Successful Interrogations") },
			{ "Leaderboards_Counter_InterrogationResultsPerformed", new CounterMeta("Stealth", "Başarılı Sorgulamalar", "Successful Interrogations") },
			{ "InterrogationsPerformed", new CounterMeta("Stealth", "Sorgulanan Düşmanlar", "Interrogations Performed") },
			{ "Leaderboards_Counter_InterrogationsPerformed", new CounterMeta("Stealth", "Sorgulanan Düşmanlar", "Interrogations Performed") },
			{ "IntimidationsPerformed", new CounterMeta("Stealth", "Korkutmalar", "Intimidations") },

			// Düşman Çeşidi / Enemy Types
			{ "HumanStuns", new CounterMeta("EnemyTypes", "Sersemletilen İnsanlar", "Humans Stunned") },

			// Yerleşkeler / Settlements
			{ "SettlementsBurned", new CounterMeta("Settlements", "Yakılan Yerleşkeler", "Settlements Burned") },
			{ "CampsCleaned", new CounterMeta("Settlements", "Temizlenen Kamplar", "Camps Cleaned") },
			{ "CampsBuilt", new CounterMeta("Settlements", "Kurulan Kamplar", "Camps Built") },
			{ "SavePointsDiscovered", new CounterMeta("Settlements", "Keşfedilen Kamp Ateşleri", "Campfires Discovered") },
			{ "Save Points Discovered", new CounterMeta("Settlements", "Keşfedilen Kamp Ateşleri", "Campfires Discovered") },

			// Sağlık & Ticaret / Trading & Wealth
			{ "TradingToken", new CounterMeta("Trading", "Trading Tokens", "Trading Tokens") },
			{ "TradingTokensGained", new CounterMeta("Trading", "Ticaret Jetonları Kazanıldı", "Trading Tokens Gained") },
			{ "TradingTokensSpent", new CounterMeta("Trading", "Ticaret Jetonları Harcandı", "Trading Tokens Spent") },

			// Trenler / Trains
			{ "TrainsDestroyed", new CounterMeta("Trains", "Yok Edilen Trenler", "Trains Destroyed") },

			// Bireysel Silahlar / Individual Weapons
			{ "FoundersBowKills", new CounterMeta("IndividualWeapons", "Ataların Yayı ile Öldürmeler", "Founder's Bow Kills") },
			{ "WinchesterKills", new CounterMeta("IndividualWeapons", "Winchester ile Öldürmeler", "Winchester Kills") },
			{ "HenryRifleKills", new CounterMeta("IndividualWeapons", "Henry Tüfeği ile Öldürmeler", "Henry Rifle Kills") },
			{ "ColtRevolverKills", new CounterMeta("IndividualWeapons", "Colt Altıpatlar ile Öldürmeler", "Colt Revolver Kills") },
			{ "HumanKillsByShotgun", new CounterMeta("IndividualWeapons", "Av Tüfeği ile Öldürmeler", "Shotgun Kills") }
		};

		public static CounterMeta GetCounterMeta(string counterId)
		{
			if (string.IsNullOrEmpty(counterId))
			{
				return new CounterMeta("Misc", "Bilinmeyen Sayaç", "Unknown Counter");
			}

			if (CounterDefinitions.TryGetValue(counterId, out var meta))
			{
				return meta;
			}

			string noSpace = counterId.Replace(" ", "");
			if (CounterDefinitions.TryGetValue(noSpace, out var metaNoSpace))
			{
				return metaNoSpace;
			}

			// Bireysel silah öldürmeleri eşleme ("Kills by ...")
			if (counterId.StartsWith("Kills by ", StringComparison.OrdinalIgnoreCase) || counterId.StartsWith("HumanKillsBy", StringComparison.OrdinalIgnoreCase))
			{
				string weaponName = counterId.StartsWith("Kills by ", StringComparison.OrdinalIgnoreCase) 
					? counterId.Substring("Kills by ".Length).Trim() 
					: counterId.Substring("HumanKillsBy".Length).Trim();
				return new CounterMeta("IndividualWeapons", weaponName + " ile Öldürmeler", "Kills by " + weaponName);
			}

			string cleanName = Patch_LocalizeManager_Localize.SplitCamelCase(noSpace);
			return new CounterMeta("Misc", cleanName, cleanName);
		}

				[HarmonyPrefix]
		private static bool Prefix(LeaderboardTab __instance, object infoResponce)
		{
			if (__instance == null || infoResponce == null) return true;

			try
			{
				FieldInfo countersField = infoResponce.GetType().GetField("Counters");
				if (countersField == null) return true;
				IDictionary counters = countersField.GetValue(infoResponce) as IDictionary;
				if (counters == null) return true;

				bool isTurkish = Patch_LocalizeManager_Localize.IsCurrentLanguageTurkish();

				// StatCountersManager ayarlarını senkronize et
				if (StatCountersManager.Instance != null && StatCountersManager.Instance.SettingsBundle != null)
				{
					var settingsList = StatCountersManager.Instance.SettingsBundle.Settings;
					if (settingsList != null)
					{
						foreach (var kvp in CounterDefinitions)
						{
							string key = kvp.Key;
							CounterMeta meta = kvp.Value;
							var existing = settingsList.Find(x => x != null && string.Equals(x.Id, key, StringComparison.OrdinalIgnoreCase));
							if (existing == null)
							{
								settingsList.Add(new StatCounterSettings
								{
									Id = key,
									DisplayName = isTurkish ? meta.TrName : meta.EnName,
									CategoryLocId = meta.CategoryKey,
									DisplayValueAsPercent = meta.IsPercent,
									StatisticsIds = new List<string> { key },
									Rules = new List<StatCounterRule>()
								});
							}
							else
							{
								existing.CategoryLocId = meta.CategoryKey;
								existing.DisplayValueAsPercent = meta.IsPercent;
							}
						}
					}
				}

				// 1. Önce tüm counter'ları __instance.View'e kaydet
				FieldInfo rankField = null;
				FieldInfo numRanksField = null;
				var counterDataList = new List<(string CounterId, CounterMeta Meta, int PlayerRank, int NumRanks)>();

				foreach (DictionaryEntry entry in counters)
				{
					string cId = entry.Key as string;
					if (string.IsNullOrEmpty(cId)) continue;
					object val = entry.Value;
					int pRank = -1;
					int numR = 0;
					if (val != null)
					{
						if (rankField == null) rankField = val.GetType().GetField("PlayerRank");
						if (numRanksField == null) numRanksField = val.GetType().GetField("NumRanks");
						if (rankField != null) pRank = (int)rankField.GetValue(val);
						if (numRanksField != null) numR = (int)numRanksField.GetValue(val);
					}
					var leaderboardCounter = __instance.View.AddOrUpdateCounter(cId, pRank, numR);
					var meta = GetCounterMeta(cId);
					if (leaderboardCounter != null)
					{
						leaderboardCounter.DisplayValueAsPercent = meta.IsPercent;
					}
					counterDataList.Add((cId, meta, pRank, numR));
				}

				// 2. Kategoriler düzenli ve sıralı şekilde oluşturulur
				var rootNodes = new ObservableList<TreeNode<TreeViewItem>>();

				// Kategori bazlı sayaç listeleri
				var categoryBuckets = new Dictionary<string, List<(string CounterId, CounterMeta Meta, int PlayerRank, int NumRanks)>>(StringComparer.OrdinalIgnoreCase);
				foreach (var cat in CategoryOrder)
				{
					categoryBuckets[cat.Key] = new List<(string, CounterMeta, int, int)>();
				}

				foreach (var item in counterDataList)
				{
					if (!categoryBuckets.ContainsKey(item.Meta.CategoryKey))
					{
						categoryBuckets[item.Meta.CategoryKey] = new List<(string, CounterMeta, int, int)>();
					}
					categoryBuckets[item.Meta.CategoryKey].Add(item);
				}

				// Sıralı kategorileri düğüm olarak ağaca ekle
				foreach (var cat in CategoryOrder)
				{
					if (!categoryBuckets.TryGetValue(cat.Key, out var bucket) || bucket.Count == 0)
					{
						continue;
					}

					string catTitle = isTurkish ? cat.Tr : cat.En;
					var catItem = new TreeViewItem(catTitle, null);
					var catNode = new TreeNode<TreeViewItem>(catItem);
					catNode.Nodes = new ObservableList<TreeNode<TreeViewItem>>();
					catNode.IsExpanded = true;

					foreach (var item in bucket)
					{
						string itemName = isTurkish ? item.Meta.TrName : item.Meta.EnName;
						if (item.PlayerRank != -1)
						{
							itemName += $" ({item.PlayerRank + 1})";
						}
						var itemNode = new TreeNode<TreeViewItem>(new TreeViewItem(itemName, null, item.CounterId));
						catNode.Nodes.Add(itemNode);
					}

					rootNodes.Add(catNode);
				}

				__instance.Selector.Nodes = rootNodes;

				var onCounterSelectedMethod = AccessTools.Method(typeof(LeaderboardTab), "OnCounterSelected");
				if (onCounterSelectedMethod != null)
				{
					var onCounterSelectedDelegate = (UnityAction<TreeNode<TreeViewItem>>)Delegate.CreateDelegate(typeof(UnityAction<TreeNode<TreeViewItem>>), __instance, onCounterSelectedMethod);
					__instance.Selector.NodeSelected.AddListener(onCounterSelectedDelegate);
				}

				var tryToSelectCounterMethod = AccessTools.Method(typeof(LeaderboardTab), "TryToSelectCounter");
				tryToSelectCounterMethod?.Invoke(__instance, new object[] { __instance.ParentWindow != null ? __instance.ParentWindow.SelectedCounterName : null });

				return false;
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogError("SetupSelectorAndCounters error: " + ex);
			}
			return true;
		}
	}
}
