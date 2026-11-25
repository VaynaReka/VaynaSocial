using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000024 RID: 36
	[HarmonyPatch(typeof(SocialEvent), "HasFailedCompletionEndTime")]
	internal static class Patch_SocialEvent_HasFailedCompletionEndTime
	{
		// Token: 0x0600006B RID: 107 RVA: 0x00006680 File Offset: 0x00004880
		[HarmonyPrefix]
		private static bool Prefix(ref bool __result)
		{
			__result = false;
			return false;
		}
	}
}
