using System;
using HarmonyLib;
using UnityEngine;

namespace VaynaSocial
{
	/// <summary>
	/// Yeni oyun başlatılırken (SandboxFlow.NameSelection) isim seçimi diyaloğunun
	/// bağlanacağı karakterin (nameSelectionEntity) null kalmasını veya yok edilmesini
	/// engelleyen koruma yaması.
	/// </summary>
	[HarmonyPatch(typeof(SandboxFlow.NameAndSpawnSelectionStateDescription), "NameSelection")]
	internal static class Patch_SandboxFlow_NameSelection
	{
		[HarmonyPrefix]
		private static void Prefix(SandboxFlow.NameAndSpawnSelectionStateDescription __instance, GameObject setup)
		{
			try
			{
				Traverse traverse = Traverse.Create(__instance);
				Entity nameEntity = traverse.Field<Entity>("nameSelectionEntity").Value;
				if (nameEntity == null || nameEntity.gameObject == null)
				{
					VaynaSocialPlugin.Log?.LogWarning("[SandboxFlowFix] nameSelectionEntity null tespit edildi! Yeni oyun için otomatik kurtarılıyor...");

					Entity[] entities = UnityEngine.Object.FindObjectsOfType<Entity>();
					if (entities != null && entities.Length > 0)
					{
						for (int i = 0; i < entities.Length; i++)
						{
							Entity ent = entities[i];
							if (ent != null && ent.gameObject != null)
							{
								traverse.Field<Entity>("nameSelectionEntity").Value = ent;
								traverse.Field<Entity>("spawnSelectionEntity").Value = ent;
								VaynaSocialPlugin.Log?.LogInfo("[SandboxFlowFix] Kurtarma başarılı: " + ent.name + " seçildi.");
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("[SandboxFlowFix] Hata: " + ex.Message);
			}
		}
	}
}
