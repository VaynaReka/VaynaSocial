using System;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000017 RID: 23
	[HarmonyPatch(typeof(ItemsDB), "FindItemByPrefabName", new Type[]
	{
		typeof(string),
		typeof(bool)
	})]
	internal static class Patch_ItemsDB_FindItemByPrefabName
	{
		// Token: 0x06000057 RID: 87 RVA: 0x00004F08 File Offset: 0x00003108
		[HarmonyPrefix]
		private static bool Prefix(ItemsDB __instance, string prefabName, ref ItemBase __result)
		{
			if (__instance == null || __instance.Items == null || string.IsNullOrEmpty(prefabName))
			{
				return true;
			}
			string text = prefabName.Replace('\\', '/').Trim();
			string text2 = text.Contains("/") ? text.Substring(text.LastIndexOf('/') + 1).Trim() : text;
			if (string.IsNullOrEmpty(Regex.Replace(text2, "[^a-zA-Z0-9]", "").ToLowerInvariant()))
			{
				return true;
			}
			for (int i = 0; i < __instance.Items.Length; i++)
			{
				ItemBase itemBase = __instance.Items[i];
				if (!(itemBase == null))
				{
					string a = itemBase.name ?? "";
					string text3 = (itemBase.Prefab != null) ? itemBase.Prefab.name : "";
					string text4 = (itemBase.PrefabPath != null) ? itemBase.PrefabPath.Replace('\\', '/') : "";
					if (string.Equals(a, prefabName, StringComparison.OrdinalIgnoreCase) || string.Equals(a, text2, StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrEmpty(text3) && (string.Equals(text3, prefabName, StringComparison.OrdinalIgnoreCase) || string.Equals(text3, text2, StringComparison.OrdinalIgnoreCase))) || (!string.IsNullOrEmpty(text4) && (string.Equals(text4, text, StringComparison.OrdinalIgnoreCase) || string.Equals(text4, text2, StringComparison.OrdinalIgnoreCase))))
					{
						__result = itemBase;
						return false;
					}
				}
			}
			return true;
		}
	}
}
