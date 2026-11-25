using System;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VaynaSocial
{
	// Token: 0x0200001B RID: 27
	[HarmonyPatch(typeof(FeaturesSocialControl), "FillProfileInformation")]
	internal static class Patch_FeaturesSocialControl_FillProfileInformation
	{
		// Token: 0x06000060 RID: 96 RVA: 0x00006240 File Offset: 0x00004440
		[HarmonyPrefix]
		private static bool Prefix(string jsonProfile)
		{
			try
			{
				FeaturesSocialControl.Profile = JsonUtility.FromJson<FeaturesSocialControl.PlayerProfile>(jsonProfile);
				if (FeaturesSocialControl.Profile != null)
				{
					VaynaSocialPlugin.ConsolidatePlayerProfile(FeaturesSocialControl.Profile);
					if (FeaturesSocialControl.Profile.Items != null && ItemsDB.Instance != null)
					{
						foreach (FeaturesSocialControl.PlayerProfile.ItemDescription itemDescription in FeaturesSocialControl.Profile.Items)
						{
							if (itemDescription != null && !string.IsNullOrEmpty(itemDescription.Prefab))
							{
								itemDescription.ItemPrefab = ItemsDB.Instance.FindItemByPrefabName(itemDescription.Prefab, true);
							}
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
						log.LogWarning("FillProfileInformation hatası: " + ex.Message);
					}
				}
			}
			return false;
		}
	}
}
