using System;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(SocialBarterInfo), "Initialize")]
	internal static class Patch_SocialBarterInfo_Initialize
	{
		[HarmonyPostfix]
		private static void Postfix(SocialBarterInfo __instance)
		{
			try
			{
				TradingEngine.Initialize();
				if (__instance != null)
				{
					__instance.MaxOffers = 10;
					__instance.SetupFee = 0f;
					__instance.SellTax = 0.05f;
					__instance.PlacedItems = TradingEngine.PlacedOffers.ToArray();
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("Patch_SocialBarterInfo_Initialize error: " + ex.Message);
			}
		}
	}
}
