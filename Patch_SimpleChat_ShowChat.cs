using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x0200002F RID: 47
	[HarmonyPatch(typeof(SimpleChat), "ShowChat")]
	internal static class Patch_SimpleChat_ShowChat
	{
		// Token: 0x06000079 RID: 121 RVA: 0x00006BF4 File Offset: 0x00004DF4
		[HarmonyPrefix]
		private static void Prefix(SimpleChat __instance)
		{
			if (__instance == null)
			{
				return;
			}
			__instance.gameObject.SetActive(true);
			if (__instance.Canvas != null)
			{
				__instance.Canvas.enabled = true;
			}
			Patch_SimpleChat_Update.EnsureChatInitialized(__instance);
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00006C2C File Offset: 0x00004E2C
		[HarmonyPostfix]
		private static void Postfix(SimpleChat __instance)
		{
			if (__instance == null)
			{
				return;
			}
			if (__instance.Canvas != null)
			{
				__instance.Canvas.enabled = true;
			}
			if (__instance.FullChat != null)
			{
				__instance.FullChat.SetActive(true);
			}
			if (__instance.CurrentInput != null)
			{
				__instance.CurrentInput.gameObject.SetActive(true);
				__instance.CurrentInput.ActivateInputField();
			}

			try
			{
				// Sohbet acildiginda hemen kabile sunucusunu sorgula
				ChatEngine.Instance?.PollOnce();

				// Hafizadaki tum kabile mesajlarini oyun ici sohbete aninda dok
				Patch_SimpleChat_Update.SyncRecentMessagesToChat(__instance);
			}
			catch
			{
			}
		}
	}
}
