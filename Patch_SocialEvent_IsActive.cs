using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000025 RID: 37
	[HarmonyPatch(typeof(SocialEvent), "IsActive")]
	internal static class Patch_SocialEvent_IsActive
	{
		// Token: 0x0600006C RID: 108 RVA: 0x00006686 File Offset: 0x00004886
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
