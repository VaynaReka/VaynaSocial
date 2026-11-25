using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VaynaSocial
{
	// Token: 0x0200002B RID: 43
	[HarmonyPatch(typeof(SocialEventsManager), "UpdateEvents")]
	internal static class Patch_SocialEventsManager_UpdateEvents
	{
		// Token: 0x06000072 RID: 114 RVA: 0x000067E4 File Offset: 0x000049E4
		[HarmonyPrefix]
		private static void Prefix(SocialEventsManager __instance)
		{
			if (__instance == null)
			{
				return;
			}
			Patch_SocialEventsManager_UpdateEvents.EnsureSettingsLoaded(__instance);
		}

		// Token: 0x06000073 RID: 115 RVA: 0x000067F8 File Offset: 0x000049F8
		[HarmonyPostfix]
		private static void Postfix(SocialEventsManager __instance)
		{
			if (__instance == null)
			{
				return;
			}
			Patch_SocialEventsManager_UpdateEvents.EnsureSettingsLoaded(__instance);
			if (__instance.SettingsBundle != null && __instance.SettingsBundle.Settings != null)
			{
				if (__instance.Events == null)
				{
					__instance.Events = new List<SocialEvent>();
				}
				using (List<SocialEventSettings>.Enumerator enumerator = __instance.SettingsBundle.Settings.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						SocialEventSettings setting = enumerator.Current;
						if (setting != null && !string.IsNullOrEmpty(setting.Id))
						{
							SocialEvent socialEvent = __instance.Events.Find((SocialEvent e) => e != null && e.Settings != null && e.Settings.Id == setting.Id);
							if (socialEvent == null)
							{
								SocialEvent socialEvent2 = new SocialEvent(setting)
								{
									StartTime = DateTime.UtcNow.AddMinutes(-30.0),
									EndTime = DateTime.UtcNow.AddDays(30.0),
									CooldownEndTime = DateTime.UtcNow.AddDays(31.0),
									CompletionEndTime = DateTime.MaxValue,
									Accepted = true,
									TurnedIn = false
								};
								try
								{
									socialEvent2.Reset();
								}
								catch
								{
								}
								socialEvent2.StartTime = DateTime.UtcNow.AddMinutes(-30.0);
								socialEvent2.EndTime = DateTime.UtcNow.AddDays(30.0);
								socialEvent2.CooldownEndTime = DateTime.UtcNow.AddDays(31.0);
								socialEvent2.CompletionEndTime = DateTime.MaxValue;
								socialEvent2.Accepted = true;
								socialEvent2.TurnedIn = false;
								__instance.Events.Add(socialEvent2);
								socialEvent = socialEvent2;
							}
							else
							{
								socialEvent.CompletionEndTime = DateTime.MaxValue;
								if (socialEvent.EndTime < DateTime.UtcNow)
								{
									socialEvent.StartTime = DateTime.UtcNow.AddMinutes(-30.0);
									socialEvent.EndTime = DateTime.UtcNow.AddDays(30.0);
									socialEvent.CooldownEndTime = DateTime.UtcNow.AddDays(31.0);
									socialEvent.CompletionEndTime = DateTime.MaxValue;
									socialEvent.TurnedIn = false;
								}
							}

							// Tracker'ları senkronize et ve asla 0 olmamasını garantile
							if (socialEvent != null)
							{
								socialEvent.Settings = setting;
								if (socialEvent.ObjectivesTrackers == null)
								{
									socialEvent.ObjectivesTrackers = new List<SocialEvent.SocialEventObjectiveTracker>();
								}
								if (setting.Objectives != null)
								{
									for (int oi = 0; oi < setting.Objectives.Count; oi++)
									{
										var obj = setting.Objectives[oi];
										if (obj == null) continue;
										int target = Patch_SocialEventCard_UpdateVisuals.GetEventTargetCount(setting.Id, obj.CounterId, obj.QuantityMin);
										if (oi < socialEvent.ObjectivesTrackers.Count)
										{
											var trk = socialEvent.ObjectivesTrackers[oi];
											if (trk != null)
											{
												trk.CounterId = obj.CounterId;
												if (trk.TargetVal <= 0)
												{
													trk.TargetVal = target;
												}
											}
										}
										else
										{
											var newTrk = new SocialEvent.SocialEventObjectiveTracker(obj.CounterId)
											{
												TargetVal = target,
												CurrentVal = 0
											};
											socialEvent.ObjectivesTrackers.Add(newTrk);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00006A88 File Offset: 0x00004C88
		public static void EnsureSettingsLoaded(SocialEventsManager mgr)
		{
			if (mgr == null)
			{
				return;
			}
			if (mgr.SettingsBundle == null || mgr.SettingsBundle.Settings == null || mgr.SettingsBundle.Settings.Count == 0)
			{
				try
				{
					string cachedEventsJson = CacheManager.GetCachedEventsJson();
					if (!string.IsNullOrEmpty(cachedEventsJson))
					{
						mgr.SettingsBundle = JsonUtility.FromJson<SocialEventSettingsBundle>(cachedEventsJson);
						if (mgr.SettingsBundle != null && mgr.SettingsBundle.Settings != null)
						{
							foreach (SocialEventSettings socialEventSettings in mgr.SettingsBundle.Settings)
							{
								if (socialEventSettings != null)
								{
									socialEventSettings.PostGet();
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (ModConfig.DebugLog.Value)
					{
						ManualLogSource log = VaynaSocialPlugin.Log;
						if (log != null)
						{
							log.LogWarning("EnsureSettingsLoaded hatasi: " + ex.Message);
						}
					}
				}
			}
		}
	}
}
