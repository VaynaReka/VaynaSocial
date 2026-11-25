using System;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(FeaturesSocialControl_Trading), "UpdateData")]
	internal static class Patch_FeaturesSocialControl_Trading_UpdateData
	{
		[HarmonyPrefix]
		private static void Prefix(FeaturesSocialControl_Trading __instance)
		{
			try
			{
				TradingEngine.Initialize();
				TradingEngine.EnsureBarterAuctionItem();
				TradingEngine.EnsureBarterPlacedItems();

				if (SocialManager.Profile != null)
				{
					VaynaSocialPlugin.ConsolidateProfile(SocialManager.Profile);
					if (SocialManager.Profile.Items != null && ItemsDB.Instance != null)
					{
						for (int i = SocialManager.Profile.Items.Count - 1; i >= 0; i--)
						{
							var item = SocialManager.Profile.Items[i];
							if (item == null)
							{
								SocialManager.Profile.Items.RemoveAt(i);
								continue;
							}
							if (item.ItemPrefab == null && !string.IsNullOrEmpty(item.Prefab))
							{
								item.ItemPrefab = ItemsDB.Instance.FindItemByPrefabName(item.Prefab, true);
							}
							if (item.ItemPrefab == null && !item.IsTimeBased)
							{
								SocialManager.Profile.Items.RemoveAt(i);
							}
						}
					}
				}

				if (FeaturesSocialControl.Profile != null)
				{
					VaynaSocialPlugin.ConsolidatePlayerProfile(FeaturesSocialControl.Profile);
					if (FeaturesSocialControl.Profile.Items != null && ItemsDB.Instance != null)
					{
						for (int i = FeaturesSocialControl.Profile.Items.Count - 1; i >= 0; i--)
						{
							var item = FeaturesSocialControl.Profile.Items[i];
							if (item == null)
							{
								FeaturesSocialControl.Profile.Items.RemoveAt(i);
								continue;
							}
							if (item.ItemPrefab == null && !string.IsNullOrEmpty(item.Prefab))
							{
								item.ItemPrefab = ItemsDB.Instance.FindItemByPrefabName(item.Prefab, true);
							}
							if (item.ItemPrefab == null && !item.IsTimeBased)
							{
								FeaturesSocialControl.Profile.Items.RemoveAt(i);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("Patch_FeaturesSocialControl_Trading_UpdateData prefix error: " + ex.Message);
			}
		}
	}
}
