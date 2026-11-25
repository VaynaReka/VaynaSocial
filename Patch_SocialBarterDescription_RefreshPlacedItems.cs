using System;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(FeaturesSocialControl.SocialBarterDescription), "RefreshPlacedItems")]
	internal static class Patch_SocialBarterDescription_RefreshPlacedItems
	{
		[HarmonyPrefix]
		private static bool Prefix(FeaturesSocialControl.SocialBarterDescription __instance, Action onCompete)
		{
			try
			{
				TradingEngine.Initialize();
				if (__instance != null)
				{
					// FeaturesSocialControl.SocialBarterDescription.PutItemDesc listesi olustur
					var list = new System.Collections.Generic.List<FeaturesSocialControl.SocialBarterDescription.PutItemDesc>();
					foreach (var item in TradingEngine.PlacedOffers)
					{
						if (item == null) continue;
						var desc = new FeaturesSocialControl.SocialBarterDescription.PutItemDesc
						{
							GUID = item.GUID,
							PlayerId = item.PlayerId,
							Buying = item.Buying,
							Stack = item.Stack,
							Item = new FeaturesSocialControl.SocialBarterDescription.PutItemDesc.ItemAmountDescription
							{
								Prefab = item.Item != null ? item.Item.Prefab : "Token Item",
								Stack = item.Item != null ? item.Item.Stack : 1
							}
						};
						if (item.ExchangeTo != null && item.ExchangeTo.Length > 0)
						{
							desc.ExchangeTo = new FeaturesSocialControl.SocialBarterDescription.PutItemDesc.ItemAmountDescription[item.ExchangeTo.Length];
							for (int i = 0; i < item.ExchangeTo.Length; i++)
							{
								desc.ExchangeTo[i] = new FeaturesSocialControl.SocialBarterDescription.PutItemDesc.ItemAmountDescription
								{
									Prefab = item.ExchangeTo[i].Prefab,
									Stack = item.ExchangeTo[i].Stack
								};
							}
						}
						list.Add(desc);
					}
					__instance.PlacedItems = list.ToArray();
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
				VaynaSocialPlugin.Log?.LogWarning("Patch_SocialBarterDescription_RefreshPlacedItems error: " + ex.Message);
				return true;
			}
		}
	}
}
