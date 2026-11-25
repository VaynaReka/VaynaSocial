using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000021 RID: 33
	[HarmonyPatch(typeof(Mailbox_Compose), "Update")]
	internal static class Patch_Mailbox_Compose_Update
	{
		// Token: 0x06000067 RID: 103 RVA: 0x0000652C File Offset: 0x0000472C
		[HarmonyPostfix]
		private static void Postfix(Mailbox_Compose __instance)
		{
			if (__instance == null || __instance.Root == null || !__instance.Root.activeSelf)
			{
				return;
			}
			if (__instance.AttachItems != null && !__instance.AttachItems.interactable)
			{
				__instance.AttachItems.interactable = true;
			}
			if (__instance.SendButton == null || __instance.SendTo == null)
			{
				return;
			}
			if (string.IsNullOrEmpty((__instance.SendTo.text != null) ? __instance.SendTo.text.Trim() : ""))
			{
				return;
			}
			if (!__instance.SendButton.interactable)
			{
				__instance.SendButton.interactable = true;
			}
		}
	}
}
