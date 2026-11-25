using System;
using System.Collections.Generic;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(SocialEventsWindow), "OnEnable")]
	internal static class Patch_SocialEventsWindow_OnEnable
	{
		[HarmonyPrefix]
		private static void Prefix(SocialEventsWindow __instance)
		{
			try
			{
				FixAllEventTrackers();
			}
			catch {}
		}

		[HarmonyPostfix]
		private static void Postfix(SocialEventsWindow __instance)
		{
			if (__instance == null)
			{
				return;
			}
			try
			{
				if (SocialEventsManager.Instance != null)
				{
					Patch_SocialEventsManager_UpdateEvents.EnsureSettingsLoaded(SocialEventsManager.Instance);
					SocialEventsManager.Instance.UpdateEvents();
					FixAllEventTrackers();
				}
				__instance.UpdateCards();
			}
			catch {}
		}

		public static void FixAllEventTrackers()
		{
			if (SocialEventsManager.Instance == null || SocialEventsManager.Instance.Events == null) return;

			foreach (var evt in SocialEventsManager.Instance.Events)
			{
				if (evt == null) continue;
				string eventId = evt.Settings != null ? (evt.Settings.Id ?? "") : "";

				if (evt.ObjectivesTrackers != null)
				{
					for (int i = 0; i < evt.ObjectivesTrackers.Count; i++)
					{
						var trk = evt.ObjectivesTrackers[i];
						if (trk == null) continue;

						if (trk.TargetVal <= 0)
						{
							int qMin = 0;
							if (evt.Settings != null && evt.Settings.Objectives != null && i < evt.Settings.Objectives.Count)
							{
								qMin = evt.Settings.Objectives[i].QuantityMin;
							}
							trk.TargetVal = Patch_SocialEventCard_UpdateVisuals.GetEventTargetCount(eventId, trk.CounterId, qMin);
							if (trk.TargetVal <= 0) trk.TargetVal = 3;
						}
					}
				}
			}
		}
	}
}
