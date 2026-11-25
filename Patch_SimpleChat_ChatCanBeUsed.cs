using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x0200002D RID: 45
	[HarmonyPatch(typeof(SimpleChat), "ChatCanBeUsed")]
	internal static class Patch_SimpleChat_ChatCanBeUsed
	{
		// Token: 0x06000076 RID: 118 RVA: 0x00006BD8 File Offset: 0x00004DD8
		[HarmonyPrefix]
		private static bool Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}
}
