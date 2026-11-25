using System;
using Achievements;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace VaynaSocial
{
	// Token: 0x02000012 RID: 18
	[HarmonyPatch(typeof(SocialManager), "UpdateProfile")]
	internal static class Patch_SocialManager_UpdateProfile
	{
		// Token: 0x06000051 RID: 81 RVA: 0x00004C38 File Offset: 0x00002E38
		[HarmonyPrefix]
		private static bool Prefix(Action onComplete)
		{
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			MegaChakaConnection.CallMethod(MegaChakaConnection.EServiceType.PlayerService, "GetProfile", string.Empty, delegate(string response)
			{
				try
				{
					if (string.IsNullOrEmpty(response) || response.Length < 10)
					{
						response = CacheManager.GetCachedProfileJson();
					}
					SocialManager.Profile = JsonUtility.FromJson<SocialPlayerProfile>(response);
					if (SocialManager.Profile == null || SocialManager.Profile.Items == null || SocialManager.Profile.Items.Count == 0)
					{
						string fallbackJson = CacheManager.GetCachedProfileJson();
						if (!string.IsNullOrEmpty(fallbackJson) && fallbackJson != response)
						{
							SocialManager.Profile = JsonUtility.FromJson<SocialPlayerProfile>(fallbackJson);
						}
					}

					TradingEngine.EnsureBarterAuctionItem();

					if (SocialManager.Profile != null)
					{
						VaynaSocialPlugin.ConsolidateProfile(SocialManager.Profile);
						if (SocialManager.Profile.Items != null && ItemsDB.Instance != null)
						{
							foreach (SocialPlayerProfile.ItemDescription itemDescription in SocialManager.Profile.Items)
							{
								if (itemDescription != null && !string.IsNullOrEmpty(itemDescription.Prefab))
								{
									itemDescription.ItemPrefab = ItemsDB.Instance.FindItemByPrefabName(itemDescription.Prefab, true);
									if (itemDescription.AdditionalItems != null)
									{
										foreach (SocialPlayerProfile.ItemDescription itemDescription2 in itemDescription.AdditionalItems)
										{
											if (itemDescription2 != null && !string.IsNullOrEmpty(itemDescription2.Prefab))
											{
												itemDescription2.ItemPrefab = ItemsDB.Instance.FindItemByPrefabName(itemDescription2.Prefab, true);
											}
										}
									}
								}
							}
						}
					}
					if (SocialManager.Profile != null)
					{
						AchievementsManager.OnSpecialAchievementUpdate("ACH_NAME_gain_many_SP", SocialManager.Profile.SP);
						if (SocialManager.Barter != null && SocialManager.Barter.AuctionItem != null)
						{
							int tokenCount = SocialManager.Profile.GetItemCount(SocialManager.Barter.AuctionItem);
							AchievementsManager.OnSpecialAchievementUpdate("ACH_NAME_earn_social_tokens", tokenCount);
							if (SocialWindow.Instance != null && SocialWindow.Instance.TokensAmountText != null)
							{
								SocialWindow.Instance.TokensAmountText.text = tokenCount.ToString();
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (ModConfig.DebugLog.Value)
					{
						ManualLogSource log = VaynaSocialPlugin.Log;
						if (log != null)
						{
							log.LogWarning("SocialManager.UpdateProfile safe handler: " + ex.Message);
						}
					}
				}
				try
				{
					Action onComplete2 = onComplete;
					if (onComplete2 != null)
					{
						onComplete2();
					}
				}
				catch
				{
				}
				try
				{
					UnityEvent profileUpdated = SocialManager.ProfileUpdated;
					if (profileUpdated != null)
					{
						profileUpdated.Invoke();
					}
				}
				catch
				{
				}
			}, null);
			return false;
		}
	}
}
