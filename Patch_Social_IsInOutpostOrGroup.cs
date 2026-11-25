using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace VaynaSocial
{
	/// <summary>
	/// Oyuncunun kamptayken veya haritada ISTIF (Online Stash) ve envanter
	/// aktariminda "KARAKOL'dan veya arkadas canlisi kamptan cok uzaktasiniz" engeline
	/// takilmasini onler; kamp/karakol kontrolunu her zaman basarili (true) dondurur.
	/// Ayrica SocialStashWindow uzerindeki karartma OverlayPanel'ini devre disi birakir
	/// ve istif envanterinin haritanin her yerinde sorunsuz yuklenmesini saglar.
	/// 
	/// Ek olarak: Istiften ardisik esya alindiginda vanilla kodunun m_disableStashButtons
	/// bayragini acik birakarak butonlari kilitlemesini engeller; arayuzden cikip girmeden
	/// sinirsiz sayida farkli tur esyanin pes pese alinabilmesini saglar.
	/// </summary>
	[HarmonyPatch(typeof(SocialWindow), "IsInOutpostOrGroup")]
	internal static class Patch_SocialWindow_IsInOutpostOrGroup
	{
		[HarmonyPrefix]
		private static bool Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(FeaturesSocialControl), "IsInOutpostOrGroup")]
	internal static class Patch_FeaturesSocialControl_IsInOutpostOrGroup
	{
		[HarmonyPrefix]
		private static bool Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}

	[HarmonyPatch(typeof(SocialStashWindow), "OnEnable")]
	internal static class Patch_SocialStashWindow_OnEnable
	{
		private static readonly MethodInfo UpdateAllMethod = AccessTools.Method(typeof(SocialStashWindow), "UpdateAll");
		private static readonly FieldInfo DisableStashButtonsField = AccessTools.Field(typeof(SocialStashWindow), "m_disableStashButtons");

		[HarmonyPrefix]
		private static void Prefix(SocialStashWindow __instance)
		{
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			TradingEngine.EnsureBarterAuctionItem();
			VaynaSocialPlugin.EnsureProfileLoaded();

			if (__instance != null && DisableStashButtonsField != null)
			{
				DisableStashButtonsField.SetValue(__instance, false);
			}
		}

		[HarmonyPostfix]
		private static void Postfix(SocialStashWindow __instance)
		{
			try
			{
				if (__instance != null)
				{
					if (DisableStashButtonsField != null)
					{
						DisableStashButtonsField.SetValue(__instance, false);
					}
					if (__instance.OverlayPanel != null && __instance.OverlayPanel.activeSelf)
					{
						__instance.OverlayPanel.SetActive(false);
					}
					if (UpdateAllMethod != null)
					{
						UpdateAllMethod.Invoke(__instance, null);
					}
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("SocialStashWindow.OnEnable Postfix error: " + ex.Message);
			}
		}
	}

	[HarmonyPatch(typeof(SocialStashWindow), "UpdateItemsListStash")]
	internal static class Patch_SocialStashWindow_UpdateItemsListStash
	{
		private static readonly FieldInfo DisableStashButtonsField = AccessTools.Field(typeof(SocialStashWindow), "m_disableStashButtons");

		[HarmonyPrefix]
		private static void Prefix(SocialStashWindow __instance)
		{
			try
			{
				MegaChakaConnection.Connected = true;
				MegaChakaConnection.OfflineMode = false;
				TradingEngine.EnsureBarterAuctionItem();
				VaynaSocialPlugin.EnsureProfileLoaded();

				// KRITIK COZUM: m_disableStashButtons bayragini UpdateItemsListStash cagrisindan once sifirla!
				// Vanilla oyun esya transfer ederken bu bayragi true yapar; liste yenilenirken bu bayrak true
				// oldugundan SetupItemButtonStash cagrilarini atlar ve tum butonlar olu kalir.
				if (__instance != null && DisableStashButtonsField != null)
				{
					DisableStashButtonsField.SetValue(__instance, false);
				}
			}
			catch { }
		}

		[HarmonyPostfix]
		private static void Postfix(SocialStashWindow __instance)
		{
			try
			{
				if (__instance != null && DisableStashButtonsField != null)
				{
					DisableStashButtonsField.SetValue(__instance, false);
				}
			}
			catch { }
		}
	}

	[HarmonyPatch(typeof(SocialStashWindow), "MoveItemFromStashToInventoryConfirmed")]
	internal static class Patch_SocialStashWindow_MoveItemConfirmed
	{
		private static readonly FieldInfo DisableStashButtonsField = AccessTools.Field(typeof(SocialStashWindow), "m_disableStashButtons");
		private static readonly MethodInfo UpdateItemsListStashMethod = AccessTools.Method(typeof(SocialStashWindow), "UpdateItemsListStash");

		[HarmonyPostfix]
		private static void Postfix(SocialStashWindow __instance)
		{
			try
			{
				if (__instance != null)
				{
					if (DisableStashButtonsField != null)
					{
						DisableStashButtonsField.SetValue(__instance, false);
					}
					if (UpdateItemsListStashMethod != null)
					{
						UpdateItemsListStashMethod.Invoke(__instance, null);
					}
				}
			}
			catch { }
		}
	}

	[HarmonyPatch(typeof(SocialStashWindow), "UpdateItemsListInventoryPlayer")]
	internal static class Patch_SocialStashWindow_UpdateItemsListInventoryPlayer
	{
		[HarmonyPostfix]
		private static void Postfix(SocialStashWindow __instance)
		{
			try
			{
				if (__instance != null && __instance.OverlayPanel != null && __instance.OverlayPanel.activeSelf)
				{
					__instance.OverlayPanel.SetActive(false);
				}
			}
			catch { }
		}
	}

	[HarmonyPatch(typeof(SocialStashWindow), "UpdateItemsListInventoryCamp")]
	internal static class Patch_SocialStashWindow_UpdateItemsListInventoryCamp
	{
		[HarmonyPostfix]
		private static void Postfix(SocialStashWindow __instance)
		{
			try
			{
				if (__instance != null && __instance.OverlayPanel != null && __instance.OverlayPanel.activeSelf)
				{
					__instance.OverlayPanel.SetActive(false);
				}
			}
			catch { }
		}
	}
}
