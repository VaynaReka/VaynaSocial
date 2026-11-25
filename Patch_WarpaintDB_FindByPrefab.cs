using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000018 RID: 24
	[HarmonyPatch(typeof(WarpaintDB), "FindByPrefab")]
	internal static class Patch_WarpaintDB_FindByPrefab
	{
		// Token: 0x06000058 RID: 88 RVA: 0x00005060 File Offset: 0x00003260
		[HarmonyPostfix]
		private static void Postfix(WarpaintDB __instance, string name, ref WarpaintCustomizationBase __result)
		{
			if (__result != null || string.IsNullOrEmpty(name) || WarpaintDB.Instance == null || WarpaintDB.Instance.Warpaints == null)
			{
				return;
			}
			string b = name.Replace(" ", "").ToLowerInvariant();
			foreach (WarpaintCustomizationBase warpaintCustomizationBase in WarpaintDB.Instance.Warpaints)
			{
				if (!(warpaintCustomizationBase == null))
				{
					string text = warpaintCustomizationBase.name ?? "";
					string text2 = warpaintCustomizationBase.Name ?? "";
					if (string.Equals(text, name, StringComparison.OrdinalIgnoreCase) || string.Equals(text2, name, StringComparison.OrdinalIgnoreCase) || string.Equals(text.Replace(" ", ""), b, StringComparison.OrdinalIgnoreCase) || string.Equals(text2.Replace(" ", ""), b, StringComparison.OrdinalIgnoreCase))
					{
						__result = warpaintCustomizationBase;
						return;
					}
				}
			}
			__result = WarpaintDB.Instance.GetDefault();
		}
	}
}
