using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000026 RID: 38
	[HarmonyPatch(typeof(SocialEvent), "IsActiveOrOnCooldown")]
	internal static class Patch_SocialEvent_IsActiveOrOnCooldown
	{
		// Token: 0x0600006D RID: 109 RVA: 0x00006699 File Offset: 0x00004899
		[HarmonyPrefix]
		private static bool Prefix(SocialEvent __instance, ref bool __result)
		{
			if (__instance != null)
			{
				__result = !__instance.TurnedIn;
				return false;
			}
			return true;
		}
	}
}
