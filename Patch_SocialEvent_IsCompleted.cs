using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000027 RID: 39
	[HarmonyPatch(typeof(SocialEvent), "IsCompleted")]
	internal static class Patch_SocialEvent_IsCompleted
	{
		// Token: 0x0600006E RID: 110 RVA: 0x000066AC File Offset: 0x000048AC
		[HarmonyPrefix]
		private static bool Prefix(SocialEvent __instance, ref bool __result)
		{
			if (__instance == null || __instance.ObjectivesTrackers == null || __instance.ObjectivesTrackers.Count == 0)
			{
				__result = false;
				return false;
			}
			for (int i = 0; i < __instance.ObjectivesTrackers.Count; i++)
			{
				SocialEvent.SocialEventObjectiveTracker socialEventObjectiveTracker = __instance.ObjectivesTrackers[i];
				if (socialEventObjectiveTracker == null || socialEventObjectiveTracker.TargetVal <= 0 || socialEventObjectiveTracker.CurrentVal < socialEventObjectiveTracker.TargetVal)
				{
					__result = false;
					return false;
				}
			}
			__result = true;
			return false;
		}
	}
}
