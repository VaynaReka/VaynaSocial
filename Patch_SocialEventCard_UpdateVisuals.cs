using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VaynaSocial
{
	// Token: 0x0200002A RID: 42
	[HarmonyPatch(typeof(SocialEventCard), "UpdateVisuals")]
	internal static class Patch_SocialEventCard_UpdateVisuals
	{
		public static readonly Dictionary<string, (string TrTitle, string TrDesc, string EnTitle, string EnDesc)> EventDetails =
			new Dictionary<string, (string, string, string, string)>(StringComparer.OrdinalIgnoreCase)
		{
			{ "official_light_campfires", ("Karakol Fenerleri", "Karakollardaki kamp ateşlerini yak.", "Outpost Beacon", "Light up campfires at outposts.") },
			{ "official_learn_skills_daily", ("Kabile Öğretileri", "Yeni kabile becerileri öğren.", "Tribal Teachings", "Learn any skills.") },
			{ "official_build_camps_daily", ("Kamp İnşası", "Topraklarımızda yeni kamplar kur.", "Camp Construction", "Build new camps.") },
			{ "official_disassemble_wagons", ("Vagon Parçalama", "Dost kamplardaki vagonları parçala.", "Wagon Salvage", "Disassemble wagons in friendly camps.") },
			{ "official_craft_items_daily", ("Becerikli Zanaatkar", "Herhangi bir eşya üret.", "Resourceful Crafting", "Craft any items.") },
			{ "official_pick_blueprints", ("Şema Kurtarma", "Silah şemalarını topla ve öğren.", "Blueprint Recovery", "Pick up weapon blueprints.") },
			{ "official_deals", ("Ticaret Anlaşmaları", "Ticaret karakollarında takas yap.", "Trade Agreements", "Make deals with traders in trading posts.") },
			{ "official_buy_full_stacks_social_trade", ("Pazar Alımı", "Sosyal pazardaki satış teklifini satın al.", "Market Purchase", "Buy the whole sell order in SOCIAL.") },
			{ "official_burn_towns", ("Kasaba Baskını", "Düşman kasabalarını ateşe ver.", "Town Infiltration", "Burn enemy Towns.") },
			{ "official_resettlement", ("Yeniden Yerleşim", "Yeniden yerleşim emrini kabul et.", "Resettlement Order", "Accept resettlement order.") },
			{ "official_bosses", ("Elebaşı Avı", "Düşman elebaşılarını ve şeflerini öldür.", "Boss Campaign", "Kill any bosses.") },
			{ "official_tycoon_v2", ("Patrona Direniş", "Demiryolu patronunu bu topraklardan sür.", "Tycoon Resistance", "Force the railroad tycoon to leave this land.") },
		};

		public static readonly Dictionary<string, (string Tr, string En)> ObjectiveNames =
			new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
		{
			{ "SavePointsDiscovered", ("Karakollarda kamp ateşi yak", "Light up campfires at outposts") },
			{ "Save Points Discovered", ("Karakollarda kamp ateşi yak", "Light up campfires at outposts") },
			{ "SkillsLearned", ("Herhangi bir beceri öğren", "Learn any skills") },
			{ "CampsBuilt", ("Yeni kamplar kur", "Build new camps") },
			{ "WagonsDisassembled", ("Dost kamplarda vagonları parçala", "Disassemble wagons in friendly camps") },
			{ "ItemsCrafted", ("Herhangi bir eşya üret", "Craft any items") },
			{ "BlueprintsLearned", ("Silah şemalarını topla / öğren", "Pick up weapon blueprints") },
			{ "TradesPerformed", ("Ticaret karakollarında takas yap", "Make deals with traders in trading posts") },
			{ "FullStacksBoughtOnSocialTrade", ("Sosyal pazardaki satış teklifini satın al", "Buy the whole sell order in SOCIAL") },
			{ "SettlementsBurned", ("Düşman kasabalarını ateşe ver", "Burn enemy Towns") },
			{ "RailwaySurrendersAccepted", ("Yeniden yerleşim emrini kabul et", "Accept resettlement order") },
			{ "BossKills", ("Düşman şeflerini / liderlerini öldür", "Kill any bosses") },
			{ "TLIMLRailwayVictory", ("Demiryolu patronunu bu topraklardan sür", "Defeat railroad tycoon") },
			{ "HumanKills", ("Düşman öldür", "Enemies killed") },
			{ "AnimalKills", ("Hayvan avla", "Animals hunted") },
			{ "AnimalsKilled", ("Hayvan avla", "Animals hunted") },
			{ "BowshotsHit", ("İsabetli ok atışı yap", "Hit targets with bow") },
			{ "WolvesKilled", ("Kurt avla", "Hunt wolves") },
			{ "BoarsKilled", ("Yaban domuzu avla", "Hunt wild boars") },
			{ "BearsKilled", ("Ayı avla", "Hunt bears") },
			{ "CampsCleaned", ("Düşman kamplarını temizle", "Clean enemy camps") },
		};

		public static int GetEventTargetCount(string eventId, string counterId, int defaultMin)
		{
			if (defaultMin > 0) return defaultMin;
			if (string.IsNullOrEmpty(eventId)) return 3;
			switch (eventId.ToLowerInvariant())
			{
				case "official_craft_items_daily": return 25;
				case "official_bosses": return 5;
				case "official_light_campfires": return 3;
				case "official_learn_skills_daily": return 3;
				case "official_build_camps_daily": return 2;
				case "official_disassemble_wagons": return 2;
				case "official_pick_blueprints": return 2;
				case "official_deals": return 2;
				case "official_burn_towns": return 2;
				case "official_resettlement": return 1;
				case "official_tycoon_v2": return 1;
				case "official_buy_full_stacks_social_trade": return 1;
				default: return 3;
			}
		}

		[HarmonyPrefix]
		private static void Prefix(SocialEventCard __instance)
		{
			if (__instance == null || __instance.Event == null) return;

			string eventId = __instance.Event.Settings != null ? (__instance.Event.Settings.Id ?? "") : "";

			// Hedef ve Tracker listesini kesinlikle doğru hedef sayısıyla doldur
			if (__instance.Event.Settings != null && __instance.Event.Settings.Objectives != null)
			{
				if (__instance.Event.ObjectivesTrackers == null)
				{
					__instance.Event.ObjectivesTrackers = new List<SocialEvent.SocialEventObjectiveTracker>();
				}
				for (int i = 0; i < __instance.Event.Settings.Objectives.Count; i++)
				{
					var obj = __instance.Event.Settings.Objectives[i];
					if (obj == null) continue;
					int target = GetEventTargetCount(eventId, obj.CounterId, obj.QuantityMin);
					if (i < __instance.Event.ObjectivesTrackers.Count)
					{
						var trk = __instance.Event.ObjectivesTrackers[i];
						if (trk != null)
						{
							trk.CounterId = obj.CounterId;
							if (trk.TargetVal <= 0) trk.TargetVal = target;
						}
					}
					else
					{
						__instance.Event.ObjectivesTrackers.Add(new SocialEvent.SocialEventObjectiveTracker(obj.CounterId)
						{
							TargetVal = target,
							CurrentVal = 0
						});
					}
				}
			}
		}

		[HarmonyPostfix]
		private static void Postfix(SocialEventCard __instance)
		{
			if (__instance == null || __instance.Event == null) return;

			bool isTurkish = Patch_LocalizeManager_Localize.IsCurrentLanguageTurkish();
			string eventId = __instance.Event.Settings != null ? (__instance.Event.Settings.Id ?? "") : "";

			// 1. BAŞLIK VE AÇIKLAMA TÜRKÇELEŞTİRME
			string titleOverride = null;
			string descOverride = null;
			if (EventDetails.TryGetValue(eventId, out var details))
			{
				titleOverride = isTurkish ? details.TrTitle : details.EnTitle;
				descOverride = isTurkish ? details.TrDesc : details.EnDesc;
			}

			if (__instance.HeaderText != null && !string.IsNullOrEmpty(titleOverride))
			{
				__instance.HeaderText.text = titleOverride.ToUpper();
			}

			// 2. GÖREV HEDEFLERİ (OBJECTIVES) & SAYAÇLAR
			string targetGoalSummary = "";
			if (__instance.ObjectivesTexts != null && __instance.Event.ObjectivesTrackers != null)
			{
				for (int i = 0; i < __instance.ObjectivesTexts.Length; i++)
				{
					Text txt = __instance.ObjectivesTexts[i];
					if (txt == null) continue;

					if (__instance.Event.Settings != null && __instance.Event.Settings.Objectives != null && i < __instance.Event.Settings.Objectives.Count)
					{
						txt.gameObject.SetActive(true);
					}
					else
					{
						txt.gameObject.SetActive(false);
						continue;
					}

					if (i >= __instance.Event.ObjectivesTrackers.Count) continue;

					var tracker = __instance.Event.ObjectivesTrackers[i];
					if (tracker == null) continue;

					// TargetVal güvencesi (Asla 0 kalmasın!)
					if (tracker.TargetVal <= 0)
					{
						int qMin = 0;
						if (__instance.Event.Settings != null && __instance.Event.Settings.Objectives != null && i < __instance.Event.Settings.Objectives.Count)
						{
							qMin = __instance.Event.Settings.Objectives[i].QuantityMin;
						}
						tracker.TargetVal = GetEventTargetCount(eventId, tracker.CounterId, qMin);
					}
					if (tracker.TargetVal <= 0) tracker.TargetVal = 1;

					// Görev başlığı
					string rawCid = tracker.CounterId ?? "";
					string cleanCid = rawCid;
					if (cleanCid.Contains(";"))
					{
						int semi = cleanCid.IndexOf(';');
						cleanCid = cleanCid.Substring(0, semi);
					}

					string displayName;
					if (ObjectiveNames.TryGetValue(cleanCid, out var pair))
					{
						displayName = isTurkish ? pair.Tr : pair.En;
					}
					else
					{
						displayName = Patch_LocalizeManager_Localize.SplitCamelCase(cleanCid);
					}

					// Görsel Format: Örn: "0 / 3 - Karakollarda kamp ateşi yak (Hedef: 3)"
					bool isDone = tracker.CurrentVal >= tracker.TargetVal;
					string colorTag = isDone ? "#41d2c8" : "#f73e2a";

					if (isTurkish)
					{
						txt.text = string.Format("<color={0}><b>{1} / {2}</b></color>  -  {3}  (Hedef: {2})",
							colorTag, tracker.CurrentVal, tracker.TargetVal, displayName);
						targetGoalSummary += string.Format("\n<color=#e0a840><b>Hedef:</b> {0} adet {1} yap.</color>", tracker.TargetVal, displayName);
					}
					else
					{
						txt.text = string.Format("<color={0}><b>{1} / {2}</b></color>  -  {3}  (Goal: {2})",
							colorTag, tracker.CurrentVal, tracker.TargetVal, displayName);
						targetGoalSummary += string.Format("\n<color=#e0a840><b>Goal:</b> Complete {0}x {1}.</color>", tracker.TargetVal, displayName);
					}
				}
			}

			// 3. AÇIKLAMA METNİNE HEDEF DETAYINI EKLE
			if (__instance.DescriptionText != null)
			{
				string baseDesc = !string.IsNullOrEmpty(descOverride) ? descOverride : __instance.DescriptionText.text;
				// Eski eklemeleri temizle
				int idx = baseDesc.IndexOf("\n<color=#e0a840>");
				if (idx >= 0) baseDesc = baseDesc.Substring(0, idx);

				__instance.DescriptionText.text = baseDesc + targetGoalSummary;
			}

			// 4. AKSİYON BUTONU KONTROLÜ
			if (__instance.ActionButton != null)
			{
				bool isCompleted = __instance.Event.IsCompleted();
				if (isCompleted)
				{
					__instance.ActionButton.gameObject.SetActive(true);
					Text btnText = __instance.ActionButton.GetComponentInChildren<Text>();
					if (btnText != null)
					{
						btnText.text = isTurkish ? "ÖDÜLÜ AL" : "CLAIM REWARD";
					}
				}
				else if (__instance.Event.IsAccepted())
				{
					// Henüz tamamlanmamışsa butonu gizle (kafa karıştırmasın)
					__instance.ActionButton.gameObject.SetActive(false);
				}
			}
		}
	}
}
