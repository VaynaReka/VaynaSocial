using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VaynaSocial
{
	/// <summary>
	/// Kampa eşya bırakırken (FeaturesTribeControl -> PutItemsTo) açılan ENVANTER
	/// penceresinin sol tarafının boş kalması ve sağ tarafın sıkışarak yazıların
	/// üst üste binmesi hatasını çözer.
	///
	/// Vanilla Mantığı:
	/// ItemExchangeDialogBox, varsayılan olarak 2 sütunlu (Sol: Takas/Kamp, Sağ: Oyuncu)
	/// bir takas penceresidir. Ancak kampa eşya koyma ekranında oyun sadece Sağ panele (RightListRoot)
	/// eşyaları dizer, Sol paneli ise tamamen boş bırakır. Bu yüzden ekranın sol yarısı simsiyah
	/// bir boşluk olarak kalır ve sağ panel daracık alanda sıkıştığı için eşya isimleri ile
	/// ağırlık/miktar yazıları iç içe biner.
	///
	/// Çözüm:
	/// 1) Tek taraflı durumlarda (sol boş, sağ dolu): Sol panel gizlenir, sağ panel tüm pencere
	///    genişliğine yayılarak tam ortaya hizalanır (0.04 - 0.96 anchor). Böylece hem boşluk kalmaz
	///    hem de eşya isimleri ve ağırlıklar geniş alana yayılarak ferah ve net görünür.
	/// 2) Çift taraflı takas durumlarında (at/sandık yağmalama gibi hem sol hem sağ doluysa):
	///    Her iki panel de normal 2 sütunlu orijinal düzenine döner.
	/// </summary>
	[HarmonyPatch(typeof(ItemExchangeDialogBox), "LateUpdate")]
	internal static class Patch_ItemExchangeDialogBox_LayoutFix
	{
		private static bool s_SavedDefaults = false;
		private static Vector2 s_OrigLeftMin, s_OrigLeftMax, s_OrigLeftOffsetMin, s_OrigLeftOffsetMax;
		private static Vector2 s_OrigRightMin, s_OrigRightMax, s_OrigRightOffsetMin, s_OrigRightOffsetMax;

		[HarmonyPostfix]
		private static void Postfix(ItemExchangeDialogBox __instance)
		{
			if (__instance == null) return;

			try
			{
				// Orijinal anchor ve offset değerlerini ilk açılışta yedekle
				if (!s_SavedDefaults)
				{
					if (__instance.LeftListPanel != null && __instance.RightListPanel != null)
					{
						s_OrigLeftMin = __instance.LeftListPanel.anchorMin;
						s_OrigLeftMax = __instance.LeftListPanel.anchorMax;
						s_OrigLeftOffsetMin = __instance.LeftListPanel.offsetMin;
						s_OrigLeftOffsetMax = __instance.LeftListPanel.offsetMax;

						s_OrigRightMin = __instance.RightListPanel.anchorMin;
						s_OrigRightMax = __instance.RightListPanel.anchorMax;
						s_OrigRightOffsetMin = __instance.RightListPanel.offsetMin;
						s_OrigRightOffsetMax = __instance.RightListPanel.offsetMax;

						s_SavedDefaults = true;
					}
				}

				int leftCount = (__instance.LeftListRoot != null) ? __instance.LeftListRoot.childCount : 0;
				int rightCount = (__instance.RightListRoot != null) ? __instance.RightListRoot.childCount : 0;

				// DURUM 1: Sadece Sağ Panel Dolu (Kampa eşya koyma - PutItemsTo)
				if (leftCount == 0 && rightCount > 0)
				{
					// Sol boş paneli gizle
					if (__instance.LeftListPanel != null && __instance.LeftListPanel.gameObject.activeSelf)
					{
						__instance.LeftListPanel.gameObject.SetActive(false);
					}

					// Sağ paneli tüm pencereye genişlet ve ortala
					if (__instance.RightListPanel != null)
					{
						if (!__instance.RightListPanel.gameObject.activeSelf)
						{
							__instance.RightListPanel.gameObject.SetActive(true);
						}

						__instance.RightListPanel.anchorMin = new Vector2(0.05f, 0.08f);
						__instance.RightListPanel.anchorMax = new Vector2(0.95f, 0.92f);
						__instance.RightListPanel.offsetMin = Vector2.zero;
						__instance.RightListPanel.offsetMax = Vector2.zero;
					}
				}
				// DURUM 2: Sadece Sol Panel Dolu
				else if (rightCount == 0 && leftCount > 0)
				{
					if (__instance.RightListPanel != null && __instance.RightListPanel.gameObject.activeSelf)
					{
						__instance.RightListPanel.gameObject.SetActive(false);
					}

					if (__instance.LeftListPanel != null)
					{
						if (!__instance.LeftListPanel.gameObject.activeSelf)
						{
							__instance.LeftListPanel.gameObject.SetActive(true);
						}

						__instance.LeftListPanel.anchorMin = new Vector2(0.05f, 0.08f);
						__instance.LeftListPanel.anchorMax = new Vector2(0.95f, 0.92f);
						__instance.LeftListPanel.offsetMin = Vector2.zero;
						__instance.LeftListPanel.offsetMax = Vector2.zero;
					}
				}
				// DURUM 3: Çift Taraflı Takas / İki Panel de Dolu
				else if (leftCount > 0 && rightCount > 0)
				{
					if (__instance.LeftListPanel != null)
					{
						if (!__instance.LeftListPanel.gameObject.activeSelf)
						{
							__instance.LeftListPanel.gameObject.SetActive(true);
						}
						if (s_SavedDefaults)
						{
							__instance.LeftListPanel.anchorMin = s_OrigLeftMin;
							__instance.LeftListPanel.anchorMax = s_OrigLeftMax;
							__instance.LeftListPanel.offsetMin = s_OrigLeftOffsetMin;
							__instance.LeftListPanel.offsetMax = s_OrigLeftOffsetMax;
						}
						else
						{
							__instance.LeftListPanel.anchorMin = new Vector2(0.02f, 0.08f);
							__instance.LeftListPanel.anchorMax = new Vector2(0.49f, 0.92f);
						}
					}

					if (__instance.RightListPanel != null)
					{
						if (!__instance.RightListPanel.gameObject.activeSelf)
						{
							__instance.RightListPanel.gameObject.SetActive(true);
						}
						if (s_SavedDefaults)
						{
							__instance.RightListPanel.anchorMin = s_OrigRightMin;
							__instance.RightListPanel.anchorMax = s_OrigRightMax;
							__instance.RightListPanel.offsetMin = s_OrigRightOffsetMin;
							__instance.RightListPanel.offsetMax = s_OrigRightOffsetMax;
						}
						else
						{
							__instance.RightListPanel.anchorMin = new Vector2(0.51f, 0.08f);
							__instance.RightListPanel.anchorMax = new Vector2(0.98f, 0.92f);
						}
					}
				}
			}
			catch
			{
				// UI yenileme istisnalarını sessizce yakala
			}
		}
	}
}
