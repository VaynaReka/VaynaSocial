using System;
using BepInEx.Logging;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000028 RID: 40
	[HarmonyPatch(typeof(SocialEvent), "TurnIn")]
	internal static class Patch_SocialEvent_TurnIn
	{
		// Token: 0x0600006F RID: 111 RVA: 0x0000671C File Offset: 0x0000491C
		[HarmonyPrefix]
		private static bool Prefix(SocialEvent __instance)
		{
			if (__instance == null || !__instance.IsCompleted())
			{
				ManualLogSource log = VaynaSocialPlugin.Log;
				if (log != null)
				{
					string str = "[VaynaSocial] Görev henüz tamamlanmadığı için teslim edilemez: ";
					string str2;
					if (__instance == null)
					{
						str2 = null;
					}
					else
					{
						SocialEventSettings settings = __instance.Settings;
						str2 = ((settings != null) ? settings.Id : null);
					}
					log.LogWarning(str + str2);
				}
				return false;
			}
			return true;
		}
	}
}
