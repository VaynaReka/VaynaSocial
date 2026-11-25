using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x0200001D RID: 29
	[HarmonyPatch(typeof(FeaturesSocialControl), "SetupProfile")]
	internal static class Patch_FeaturesSocialControl_SetupProfile
	{
		// Token: 0x06000063 RID: 99 RVA: 0x00006490 File Offset: 0x00004690
		[HarmonyPostfix]
		private static void Postfix(FeaturesSocialControl __instance)
		{
			if (__instance != null && __instance.Tag != null)
			{
				__instance.Tag.text = VaynaSocialPlugin.GetPlayerDisplayName();
			}
		}
	}
}
