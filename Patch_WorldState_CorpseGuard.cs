using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace VaynaSocial
{
	/// <summary>
	/// Oyunu kaydederken veya yuklerken kamplardaki cesetlerin kalici olmasini onler.
	/// Save dosyasina kamp icindeki olu bedenlerin yazilmasini engeller.
	/// </summary>
	[HarmonyPatch(typeof(WorldState), "RetrieveKnownData")]
	internal static class Patch_WorldState_RetrieveKnownData
	{
		[HarmonyPostfix]
		private static void Postfix(WorldState __instance)
		{
			try
			{
				if (__instance != null && __instance.PersistentDeadBodies != null)
				{
					// Kamplara yakin veya oyuncunun dost bolgelerindeki cesetleri save listesinden temizle
					int removed = CorpseCleanupManager.CleanupAllDeadBodies(forceAll: false);
					if (removed > 0)
					{
						VaynaSocialPlugin.Log?.LogInfo($"[SaveGuard] Kayit oncesi {removed} adet ceset temizlendi, ambarlara aktarildi.");
					}
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("Patch_WorldState_RetrieveKnownData hata: " + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(WorldState), "ApplyKnownData")]
	internal static class Patch_WorldState_ApplyKnownData
	{
		[HarmonyPostfix]
		private static void Postfix(WorldState __instance)
		{
			try
			{
				// Oyun yuklendikten hemen sonra sahnede kalan eski cesetleri supur
				if (VaynaSocialPlugin.Instance != null)
				{
					VaynaSocialPlugin.Instance.StartCoroutine(DelayedCleanupAfterLoad());
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("Patch_WorldState_ApplyKnownData hata: " + ex.Message);
			}
		}

		private static System.Collections.IEnumerator DelayedCleanupAfterLoad()
		{
			yield return new WaitForSeconds(2.0f);
			int cleaned = CorpseCleanupManager.CleanupAllDeadBodies(forceAll: false);
			if (cleaned > 0)
			{
				VaynaSocialPlugin.Log?.LogInfo($"[SaveGuard] Yukleme sonrasi {cleaned} adet eski ceset temizlendi.");
			}
		}
	}
}
