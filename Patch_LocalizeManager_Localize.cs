using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(LocalizeManager), "Localize", new Type[]
	{
		typeof(string),
		typeof(object[])
	})]
	internal static class Patch_LocalizeManager_Localize
	{
		[HarmonyPostfix]
		private static void Postfix(string tag, object[] p, ref string __result)
		{
			if (string.IsNullOrEmpty(tag) && string.IsNullOrEmpty(__result))
			{
				return;
			}
			if ((tag != null && tag.IndexOf("active listing limit", StringComparison.OrdinalIgnoreCase) >= 0) || (__result != null && __result.IndexOf("active listing limit", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				__result = (Patch_LocalizeManager_Localize.IsCurrentLanguageTurkish() ? "Maksimum 10 aktif pazar teklifi sınırına ulaştınız.\n\nYeni bir teklif oluşturmak için mevcut tekliflerinizden birini iptal edebilir veya satışın tamamlanmasını bekleyebilirsiniz." : "You have reached the 10 active listing limit.\n\nPlease cancel an existing listing or wait for completion to place a new one.");
				return;
			}
			string text = Patch_LocalizeManager_Localize.GetLeaderboardTranslation(tag, __result);
			if (!string.IsNullOrEmpty(text))
			{
				if (p != null && p.Length != 0)
				{
					try
					{
						text = string.Format(text, p);
					}
					catch
					{
					}
				}
				__result = text;
				return;
			}
			if (__result != null && __result.StartsWith("#$"))
			{
				string text2 = __result.Substring(2);
				if (text2.StartsWith("Leaderboards_Counter_"))
				{
					text2 = text2.Substring("Leaderboards_Counter_".Length);
				}
				else if (text2.StartsWith("Leaderboards_Category_"))
				{
					text2 = text2.Substring("Leaderboards_Category_".Length);
				}
				__result = Patch_LocalizeManager_Localize.SplitCamelCase(text2);
			}
		}

		private static string GetLeaderboardTranslation(string tag, string currentResult)
		{
			bool isTr = Patch_LocalizeManager_Localize.IsCurrentLanguageTurkish();
			string candidate = string.IsNullOrEmpty(tag) ? (currentResult ?? "") : tag;

			string clean = candidate;
			while (clean.StartsWith("#") || clean.StartsWith("$"))
			{
				clean = clean.Substring(1);
			}

			// 1. Kategori kontrolü
			if (clean.StartsWith("Leaderboards_Category_"))
			{
				string catKey = "$" + clean;
				if (Patch_LocalizeManager_Localize.CategoryTranslations.TryGetValue(catKey, out var catVal))
				{
					return isTr ? catVal.Item1 : catVal.Item2;
				}
				return Patch_LocalizeManager_Localize.SplitCamelCase(clean.Substring("Leaderboards_Category_".Length));
			}

			if (Patch_LocalizeManager_Localize.CategoryTranslations.TryGetValue(candidate.Replace("#$", "$"), out var directCat))
			{
				return isTr ? directCat.Item1 : directCat.Item2;
			}

			// 2. Sayaç kontrolü
			while (clean.StartsWith("Leaderboards_Counter_") || clean.StartsWith("#$") || clean.StartsWith("$"))
			{
				if (clean.StartsWith("Leaderboards_Counter_"))
				{
					clean = clean.Substring("Leaderboards_Counter_".Length);
				}
				else
				{
					clean = clean.Substring(1);
				}
			}

			if (Patch_LocalizeManager_Localize.CounterTranslations.TryGetValue(clean, out var counterVal))
			{
				return isTr ? counterVal.Item1 : counterVal.Item2;
			}

			// Parantezli sayı veya son ek kontrolü (ör: "EventsTurnedIn (23)")
			int parenIdx = clean.IndexOf('(');
			if (parenIdx > 0)
			{
				string trimmed = clean.Substring(0, parenIdx).Trim();
				if (Patch_LocalizeManager_Localize.CounterTranslations.TryGetValue(trimmed, out var trimmedVal))
				{
					string suffix = clean.Substring(parenIdx);
					return (isTr ? trimmedVal.Item1 : trimmedVal.Item2) + " " + suffix;
				}
			}

			// "Kills by ..." kalıbı
			if (candidate.StartsWith("Kills by ", StringComparison.OrdinalIgnoreCase) || clean.StartsWith("Kills by ", StringComparison.OrdinalIgnoreCase))
			{
				string weapon = candidate.StartsWith("Kills by ", StringComparison.OrdinalIgnoreCase) ? candidate.Substring("Kills by ".Length).Trim() : clean.Substring("Kills by ".Length).Trim();
				if (isTr)
				{
					if (weapon.Equals("Sinew-backed Bow", StringComparison.OrdinalIgnoreCase))
					{
						weapon = "Kirişli Yay";
					}
					else if (weapon.StartsWith("a ", StringComparison.OrdinalIgnoreCase))
					{
						weapon = weapon.Substring(2).Trim();
					}
					return weapon + " ile Öldürmeler";
				}
				return "Kills by " + weapon;
			}

			if (clean.Equals("Trading Tokens", StringComparison.OrdinalIgnoreCase) || clean.Equals("TradingTokens", StringComparison.OrdinalIgnoreCase))
			{
				return isTr ? "Ticaret Kuruşu" : "Trading Tokens";
			}

			if (currentResult != null && (currentResult.StartsWith("#$") || currentResult.StartsWith("$")))
			{
				string cleanRes = currentResult;
				while (cleanRes.StartsWith("#") || cleanRes.StartsWith("$")) cleanRes = cleanRes.Substring(1);
				while (cleanRes.StartsWith("Leaderboards_Counter_") || cleanRes.StartsWith("Leaderboards_Category_"))
				{
					if (cleanRes.StartsWith("Leaderboards_Counter_")) cleanRes = cleanRes.Substring("Leaderboards_Counter_".Length);
					else cleanRes = cleanRes.Substring("Leaderboards_Category_".Length);
				}
				if (Patch_LocalizeManager_Localize.CounterTranslations.TryGetValue(cleanRes, out var resVal))
				{
					return isTr ? resVal.Item1 : resVal.Item2;
				}
				return Patch_LocalizeManager_Localize.SplitCamelCase(cleanRes);
			}

			return null;
		}

		public static string SplitCamelCase(string input)
		{
			if (string.IsNullOrEmpty(input)) return "";
			return Regex.Replace(Regex.Replace(input, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
		}

		public static bool IsCurrentLanguageTurkish()
		{
			try
			{
				string text = LocalizeManager.CurrentLanguage;
				if (!string.IsNullOrEmpty(text))
				{
					return text.IndexOf("Turkish", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("Türkçe", StringComparison.OrdinalIgnoreCase) >= 0;
				}
			}
			catch
			{
			}
			return true;
		}

		public static readonly Dictionary<string, ValueTuple<string, string>> CategoryTranslations = new Dictionary<string, ValueTuple<string, string>>(StringComparer.OrdinalIgnoreCase)
		{
			{ "$Leaderboards_Category_Hunting", new ValueTuple<string, string>("Avlanma", "Hunting") },
			{ "$Leaderboards_Category_EnemyKills", new ValueTuple<string, string>("Düşman Öldürmeleri", "Enemy Kills") },
			{ "$Leaderboards_Category_Weapons", new ValueTuple<string, string>("Silahlar", "Weapons") },
			{ "$Leaderboards_Category_Misc", new ValueTuple<string, string>("Karışık", "Miscellaneous") },
			{ "$Leaderboards_Category_RealisticDamage", new ValueTuple<string, string>("Gerçekçi Hasar", "Realistic Damage") },
			{ "$Leaderboards_Category_IndividualWeaponKills", new ValueTuple<string, string>("Bireysel Silahlarla Düşman Öldürmeleri", "Individual Weapon Kills") },
			{ "$Leaderboards_Category_WeaponTypeKills", new ValueTuple<string, string>("Silah Türüne Göre Düşman Öldürmeleri", "Kills by Weapon Type") },
			{ "$Leaderboards_Category_Stealth", new ValueTuple<string, string>("Gizlilik", "Stealth") },
			{ "$Leaderboards_Category_EnemyTypes", new ValueTuple<string, string>("Düşman Çeşidi", "Enemy Types") },
			{ "$Leaderboards_Category_Settlements", new ValueTuple<string, string>("Yerleşkeler", "Settlements") },
			{ "$Leaderboards_Category_Health", new ValueTuple<string, string>("Sağlık & Ticaret", "Health & Trading") },
			{ "$Leaderboards_Category_Trading", new ValueTuple<string, string>("Ticaret & Varlık", "Trading & Wealth") },
			{ "$Leaderboards_Category_Trains", new ValueTuple<string, string>("Trenler", "Trains") },
			{ "$Leaderboards_Category_Challenges", new ValueTuple<string, string>("Mücadeleler & Üretim", "Challenges & Crafting") }
		};

		public static readonly Dictionary<string, ValueTuple<string, string>> CounterTranslations = new Dictionary<string, ValueTuple<string, string>>(StringComparer.OrdinalIgnoreCase)
		{
			// Avlanma
			{ "AnimalsKilled", new ValueTuple<string, string>("Toplam Hayvan Öldürmeleri", "Total Animals Killed") },
			{ "Animals Killed", new ValueTuple<string, string>("Toplam Hayvan Öldürmeleri", "Total Animals Killed") },
			{ "AnimalKills", new ValueTuple<string, string>("Toplam Hayvan Öldürmeleri", "Total Animals Killed") },
			{ "AnimalKillsDeers", new ValueTuple<string, string>("Öldürülen Geyikler", "Deers Killed") },
			{ "AnimalKillsFish", new ValueTuple<string, string>("Öldürülen Balıklar", "Fish Caught") },
			{ "AnimalKillsRabbits", new ValueTuple<string, string>("Öldürülen Tavşanlar", "Rabbits Killed") },
			{ "BearsKilled", new ValueTuple<string, string>("Öldürülen Ayılar", "Bears Killed") },
			{ "BoarsKilled", new ValueTuple<string, string>("Öldürülen Domuzlar", "Boars Killed") },
			{ "WolvesKilled", new ValueTuple<string, string>("Öldürülen Kurtlar", "Wolves Killed") },

			// Düşman Öldürmeleri
			{ "BossKills", new ValueTuple<string, string>("Boss Öldürmeleri", "Bosses Killed") },
			{ "HumanKills", new ValueTuple<string, string>("İnsan Öldürmeleri", "Human Kills") },
			{ "HumanKillsHeadshots", new ValueTuple<string, string>("Kafadan Vuruşlar", "Headshots") },

			// Silahlar
			{ "BowsAccuracy", new ValueTuple<string, string>("Yayların Doğruluğu", "Bow Accuracy") },
			{ "BowshotsFired", new ValueTuple<string, string>("Atılan Oklar", "Arrows Fired") },
			{ "BowshotsHit", new ValueTuple<string, string>("İsabet Alan Oklar", "Arrows Hit") },
			{ "BulletsFired", new ValueTuple<string, string>("Sıkılan Mermiler", "Bullets Fired") },
			{ "GunsAccuracy", new ValueTuple<string, string>("Silahların Doğruluğu", "Gun Accuracy") },
			{ "GunshotsFired", new ValueTuple<string, string>("Yapılan Atışlar", "Shots Fired") },
			{ "GunshotsHit", new ValueTuple<string, string>("İsabet Alan Atışlar", "Shots Hit") },

			// Karışık
			{ "EventsTurnedIn", new ValueTuple<string, string>("Tamamlanan Etkinlikler", "Events Completed") },
			{ "ItemsCrafted", new ValueTuple<string, string>("Üretilen Öğeler", "Items Crafted") },
			{ "SkillsLearned", new ValueTuple<string, string>("Öğrenilen Beceriler", "Skills Learned") },

			// Gerçekçi Hasar
			{ "FractureCaused", new ValueTuple<string, string>("Kırık Oldu", "Fractures Caused") },
			{ "BrokenBones", new ValueTuple<string, string>("Kırık Oldu", "Broken Bones") },
			{ "HeartWound", new ValueTuple<string, string>("Kalbinden Yaralanan İnsanlar", "Heart Wounds") },
			{ "HeavyBleedCaused", new ValueTuple<string, string>("Ağır Kanama Oldu", "Heavy Bleeds Caused") },
			{ "LightBleedCaused", new ValueTuple<string, string>("Hafif Kanama Oldu", "Light Bleeds Caused") },

			// Silah Türüne Göre Düşman Öldürmeleri
			{ "HumanKillsByBow", new ValueTuple<string, string>("Yay ile Yapılan Öldürmeler", "Kills by Bow") },
			{ "HumanKillsByKnife", new ValueTuple<string, string>("Bıçakla Yapılan Öldürmeler", "Kills by Knife") },
			{ "HumanKillsByPistol", new ValueTuple<string, string>("Tabanca ile Öldürmeler", "Kills by Pistol") },
			{ "HumanKillsByRifle", new ValueTuple<string, string>("Tüfek ile Öldürmeler", "Kills by Rifle") },
			{ "HumanKillsHeadshotsByBow", new ValueTuple<string, string>("Yay ile Kafadan Vuruşlar", "Headshots by Bow") },
			{ "HumanKillsHeadshotsByPistol", new ValueTuple<string, string>("Tabanca ile Kafadan Vuruşlar", "Headshots by Pistol") },
			{ "HumanKillsHeadshotsByRifle", new ValueTuple<string, string>("Tüfekle Kafadan Vuruşlar", "Headshots by Rifle") },

			// Gizlilik
			{ "HumanKillsStealth", new ValueTuple<string, string>("Gizli İnsan Öldürmeleri", "Stealth Human Kills") },
			{ "InterrogationsPerformed", new ValueTuple<string, string>("Sorgulanan Düşmanlar", "Interrogations Performed") },
			{ "InterrogationResultsPerformed", new ValueTuple<string, string>("Başarılı Sorgulamalar", "Successful Interrogations") },
			{ "IntimidationsPerformed", new ValueTuple<string, string>("Korkutmalar", "Intimidations") },

			// Düşman Çeşidi
			{ "HumanStuns", new ValueTuple<string, string>("Sersemletilen İnsanlar", "Humans Stunned") },

			// Yerleşkeler
			{ "SettlementsBurned", new ValueTuple<string, string>("Yakılan Yerleşkeler", "Settlements Burned") },
			{ "CampsCleaned", new ValueTuple<string, string>("Temizlenen Kamplar", "Camps Cleaned") },
			{ "CampsBuilt", new ValueTuple<string, string>("Kurulan Kamplar", "Camps Built") },
			{ "SavePointsDiscovered", new ValueTuple<string, string>("Keşfedilen Kamp Ateşleri", "Campfires Discovered") },
			{ "Save Points Discovered", new ValueTuple<string, string>("Keşfedilen Kamp Ateşleri", "Campfires Discovered") },

			// Sağlık / Ticaret
			{ "TradingToken", new ValueTuple<string, string>("Trading Tokens", "Trading Tokens") },
			{ "TradingTokensGained", new ValueTuple<string, string>("Ticaret Jetonları Kazanıldı", "Trading Tokens Gained") },
			{ "TradingTokensSpent", new ValueTuple<string, string>("Ticaret Jetonları Harcandı", "Trading Tokens Spent") },

			// Trenler
			{ "TrainsDestroyed", new ValueTuple<string, string>("Yok Edilen Trenler", "Trains Destroyed") },

			// Bireysel Silahlar
			{ "FoundersBowKills", new ValueTuple<string, string>("Ataların Yayı ile Öldürmeler", "Founder's Bow Kills") },
			{ "WinchesterKills", new ValueTuple<string, string>("Winchester ile Öldürmeler", "Winchester Kills") },
			{ "HenryRifleKills", new ValueTuple<string, string>("Henry Tüfeği ile Öldürmeler", "Henry Rifle Kills") },
			{ "ColtRevolverKills", new ValueTuple<string, string>("Colt Altıpatlar ile Öldürmeler", "Colt Revolver Kills") },
			{ "HumanKillsByShotgun", new ValueTuple<string, string>("Av Tüfeği ile Öldürmeler", "Shotgun Kills") }
		};
	}
}
