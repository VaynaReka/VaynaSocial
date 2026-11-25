using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000023 RID: 35
	[HarmonyPatch(typeof(CommandLineExtensions), "GetCommandLineOption")]
	internal static class Patch_NonSteamLoginFallback
	{
		// Token: 0x0600006A RID: 106 RVA: 0x00006661 File Offset: 0x00004861
		[HarmonyPostfix]
		private static void Postfix(string optionName, ref string __result)
		{
			if (string.IsNullOrEmpty(__result) && optionName == "login")
			{
				__result = "steam";
			}
		}
	}
}
