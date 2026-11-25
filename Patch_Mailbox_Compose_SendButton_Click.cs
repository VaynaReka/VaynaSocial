using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000022 RID: 34
	[HarmonyPatch(typeof(Mailbox_Compose), "SendButton_Click")]
	internal static class Patch_Mailbox_Compose_SendButton_Click
	{
		// Token: 0x06000068 RID: 104 RVA: 0x000065E8 File Offset: 0x000047E8
		[HarmonyPrefix]
		private static void Prefix(Mailbox_Compose __instance)
		{
			if (__instance != null && __instance.SendTo != null && !string.IsNullOrEmpty(__instance.SendTo.text))
			{
				string str = __instance.SendTo.text.Trim();
				InformationalScreen instance = InformationalScreen.Instance;
				if (instance == null)
				{
					return;
				}
				instance.AddMessage("<color=yellow>Posta gönderiliyor: " + str + "...</color>", InformationalScreen_InGame.EMessageSize.Large, 3.5f, 0.5f, false);
			}
		}

		// Token: 0x06000069 RID: 105 RVA: 0x0000665A File Offset: 0x0000485A
		[HarmonyPostfix]
		private static void Postfix()
		{
			MailEngine.RefreshMailboxAfterSend();
		}
	}
}
