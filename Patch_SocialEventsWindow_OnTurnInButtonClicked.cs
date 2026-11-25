using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000029 RID: 41
	[HarmonyPatch(typeof(SocialEventsWindow), "OnTurnInButtonClicked")]
	internal static class Patch_SocialEventsWindow_OnTurnInButtonClicked
	{
		// Token: 0x06000070 RID: 112 RVA: 0x00006769 File Offset: 0x00004969
		[HarmonyPrefix]
		private static bool Prefix(SocialEventCard card)
		{
			return !(card == null) && card.Event != null && card.Event.IsCompleted();
		}
	}
}
