using System;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(SocialBarterInfo), "RefreshPlacedItems")]
	internal static class Patch_SocialBarterInfo_RefreshPlacedItems
	{
		[HarmonyPrefix]
		private static bool Prefix(SocialBarterInfo __instance, Action onCompete)
		{
			try
			{
				TradingEngine.Initialize();
				if (__instance != null)
				{
					__instance.PlacedItems = TradingEngine.PlacedOffers.ToArray();
					if (onCompete != null)
					{
						onCompete();
					}
					__instance.PlacedItemsUpdated?.Invoke();
				}
				return false;
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("Patch_SocialBarterInfo_RefreshPlacedItems error: " + ex.Message);
				return true;
			}
		}
	}
}
