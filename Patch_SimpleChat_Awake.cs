using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x0200002E RID: 46
	[HarmonyPatch(typeof(SimpleChat), "Awake")]
	internal static class Patch_SimpleChat_Awake
	{
		// Token: 0x06000077 RID: 119 RVA: 0x00006BDE File Offset: 0x00004DDE
		[HarmonyPrefix]
		private static void Prefix()
		{
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00006BEC File Offset: 0x00004DEC
		[HarmonyPostfix]
		private static void Postfix(SimpleChat __instance)
		{
			Patch_SimpleChat_Update.EnsureChatInitialized(__instance);
		}
	}
}
