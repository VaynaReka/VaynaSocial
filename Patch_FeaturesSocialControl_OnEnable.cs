using System;
using System.Reflection;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x0200001C RID: 28
	[HarmonyPatch(typeof(FeaturesSocialControl), "OnEnable")]
	internal static class Patch_FeaturesSocialControl_OnEnable
	{
		// Token: 0x06000061 RID: 97 RVA: 0x0000632C File Offset: 0x0000452C
		[HarmonyPrefix]
		private static void Prefix(FeaturesSocialControl __instance)
		{
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			MegaChakaConnection.PlayerId = VaynaSocialPlugin.GetPlayerDisplayName();
			if (WorldState.Instance != null)
			{
				WorldState.Instance.WorldSocialBanned = false;
			}
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00006358 File Offset: 0x00004558
		[HarmonyPostfix]
		private static void Postfix(FeaturesSocialControl __instance)
		{
			if (__instance == null)
			{
				return;
			}
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			if (WorldState.Instance != null)
			{
				WorldState.Instance.WorldSocialBanned = false;
			}
			if (__instance.OnlineStatus != null)
			{
				__instance.OnlineStatus.text = "<color=#55FF55>CONNECTED (Tribal Network)</color>";
			}
			if (__instance.Tag != null)
			{
				__instance.Tag.text = VaynaSocialPlugin.GetPlayerDisplayName();
			}
			if (__instance.SocialTradingShow != null)
			{
				__instance.SocialTradingShow.interactable = true;
			}
			if (__instance.MailboxWindowShow != null)
			{
				__instance.MailboxWindowShow.interactable = true;
			}
			if (__instance.UnstuckButton != null)
			{
				__instance.UnstuckButton.interactable = true;
			}
			try
			{
				if (FeaturesSocialControl.Profile == null)
				{
					MethodInfo methodInfo = AccessTools.Method(typeof(FeaturesSocialControl), "RetrievePlayerData", null, null);
					if (methodInfo != null)
					{
						methodInfo.Invoke(__instance, new object[1]);
					}
				}
				else
				{
					MethodInfo methodInfo2 = AccessTools.Method(typeof(FeaturesSocialControl), "SetupProfile", null, null);
					if (methodInfo2 != null)
					{
						methodInfo2.Invoke(__instance, new object[]
						{
							FeaturesSocialControl.Profile
						});
					}
				}
			}
			catch
			{
			}
		}
	}
}
