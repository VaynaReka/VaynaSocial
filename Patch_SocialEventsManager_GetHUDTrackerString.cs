using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(SocialEventsManager), "GetHUDTrackerString")]
	internal static class Patch_SocialEventsManager_GetHUDTrackerString
	{
		[HarmonyPrefix]
		private static bool Prefix(SocialEventsManager __instance, ref string __result)
		{
			if (__instance == null || __instance.Events == null || __instance.Events.Count == 0)
			{
				__result = string.Empty;
				return false;
			}

			try
			{
				bool isTurkish = Patch_LocalizeManager_Localize.IsCurrentLanguageTurkish();
				StringBuilder sb = new StringBuilder();

				__instance.Events.Sort((SocialEvent a, SocialEvent b) =>
				{
					if (a == null && b == null) return 0;
					if (a == null) return 1;
					if (b == null) return -1;
					return a.TrackingPriority.CompareTo(b.TrackingPriority);
				});

				bool first = true;
				foreach (SocialEvent evt in __instance.Events)
				{
					if (evt == null || evt.TrackingPriority == 0 || (!evt.IsActive() && !evt.IsCompleted()) || evt.HasFailedCompletionEndTime() || evt.IsTurnedIn())
					{
						continue;
					}

					// Tracker'ların TargetVal güvencesi
					if (evt.Settings != null && evt.Settings.Objectives != null)
					{
						if (evt.ObjectivesTrackers == null)
						{
							evt.ObjectivesTrackers = new List<SocialEvent.SocialEventObjectiveTracker>();
						}
						for (int oi = 0; oi < evt.Settings.Objectives.Count; oi++)
						{
							var obj = evt.Settings.Objectives[oi];
							if (obj == null) continue;
							int target = Patch_SocialEventCard_UpdateVisuals.GetEventTargetCount(evt.Settings != null ? evt.Settings.Id : null, obj.CounterId, obj.QuantityMin);
							if (oi < evt.ObjectivesTrackers.Count)
							{
								var trk = evt.ObjectivesTrackers[oi];
								if (trk != null && trk.TargetVal <= 0) trk.TargetVal = target;
							}
							else
							{
								evt.ObjectivesTrackers.Add(new SocialEvent.SocialEventObjectiveTracker(obj.CounterId)
								{
									TargetVal = target,
									CurrentVal = 0
								});
							}
						}
					}

					if (!first)
					{
						sb.AppendLine();
						sb.AppendLine();
					}
					first = false;

					string eventId = evt.Settings != null ? (evt.Settings.Id ?? "") : "";
					string title = null;
					string desc = null;

					if (Patch_SocialEventCard_UpdateVisuals.EventDetails.TryGetValue(eventId, out var details))
					{
						title = isTurkish ? details.TrTitle : details.EnTitle;
						desc = isTurkish ? details.TrDesc : details.EnDesc;
					}
					else if (evt.Settings != null)
					{
						title = evt.Settings.Title;
						desc = evt.Settings.Description;
					}

					sb.Append("<size=18><color=#ffffffcc>");
					if (!string.IsNullOrEmpty(title))
					{
						sb.AppendLine(string.Format("<b>{0}</b>", title.ToUpper()));
					}
					if (!string.IsNullOrEmpty(desc))
					{
						sb.AppendLine(desc);
					}

					if (evt.ObjectivesTrackers != null)
					{
						foreach (var tracker in evt.ObjectivesTrackers)
						{
							if (tracker == null) continue;
							int target = tracker.TargetVal > 0 ? tracker.TargetVal : 1;
							string cid = tracker.CounterId ?? "";
							if (cid.Contains(";")) cid = cid.Substring(0, cid.IndexOf(';'));

							string objName;
							if (Patch_SocialEventCard_UpdateVisuals.ObjectiveNames.TryGetValue(cid, out var pair))
							{
								objName = isTurkish ? pair.Tr : pair.En;
							}
							else
							{
								objName = Patch_LocalizeManager_Localize.SplitCamelCase(cid);
							}

							bool isDone = tracker.CurrentVal >= target;
							string colorTag = isDone ? "#41d2c8" : "#f73e2a";

							if (isTurkish)
							{
								sb.AppendLine(string.Format(" - <color={0}><b>{1} / {2}</b></color> {3}  (Hedef: {2})",
									colorTag, tracker.CurrentVal, target, objName));
							}
							else
							{
								sb.AppendLine(string.Format(" - <color={0}><b>{1} / {2}</b></color> {3}  (Goal: {2})",
									colorTag, tracker.CurrentVal, target, objName));
							}
						}
					}

					sb.Append("</color></size>");
					string timeStr = evt.IsCompleted() ? (isTurkish ? "<color=#41d2c8>[TAMAMLANDI!]</color>" : "<color=#41d2c8>[COMPLETED!]</color>") : SocialEventCard.GetTimeLeftString(evt);
					sb.Append(string.Format("<size=16><color=#eeeeee99>{0}</color></size>", timeStr));
				}

				__result = sb.ToString();
				return false;
			}
			catch (Exception ex)
			{
				if (ModConfig.DebugLog.Value && VaynaSocialPlugin.Log != null)
				{
					VaynaSocialPlugin.Log.LogError("GetHUDTrackerString error: " + ex);
				}
				return true;
			}
		}
	}
}
