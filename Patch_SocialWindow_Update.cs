using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000011 RID: 17
	[HarmonyPatch(typeof(SocialWindow), "Update")]
	internal static class Patch_SocialWindow_Update
	{
		// Token: 0x06000050 RID: 80 RVA: 0x00004B9C File Offset: 0x00002D9C
		[HarmonyPostfix]
		private static void Postfix(SocialWindow __instance)
		{
			if (__instance == null)
			{
				return;
			}
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			if (__instance.NotConnectedImage != null && __instance.NotConnectedImage.gameObject.activeSelf)
			{
				__instance.NotConnectedImage.gameObject.SetActive(false);
			}
			if (__instance.SocialLimited != null && __instance.SocialLimited.activeSelf)
			{
				__instance.SocialLimited.SetActive(false);
			}
			if (WorldState.Instance != null && WorldState.Instance.WorldSocialBanned)
			{
				WorldState.Instance.WorldSocialBanned = false;
			}
		}
	}
}
