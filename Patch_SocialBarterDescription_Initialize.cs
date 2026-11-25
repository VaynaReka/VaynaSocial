using System;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(FeaturesSocialControl.SocialBarterDescription), "Initialize")]
	internal static class Patch_SocialBarterDescription_Initialize
	{
		[HarmonyPostfix]
		private static void Postfix(FeaturesSocialControl.SocialBarterDescription __instance)
		{
			try
			{
				TradingEngine.Initialize();
				if (__instance != null)
				{
					__instance.MaxOffers = 10;
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("Patch_SocialBarterDescription_Initialize error: " + ex.Message);
			}
		}
	}
}
