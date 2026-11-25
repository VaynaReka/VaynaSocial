using System;
using System.Linq;
using HarmonyLib;
using UnityEngine.Events;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(MegaChakaConnection), "CallMethod", new Type[] {
		typeof(MegaChakaConnection.EServiceType),
		typeof(string),
		typeof(string),
		typeof(UnityAction<string>),
		typeof(UnityAction<byte[]>)
	})]
	internal static class Patch_MegaChakaConnection_CallMethod
	{
		[HarmonyPrefix]
		[HarmonyPriority(800)]
		private static bool Prefix(
			MegaChakaConnection.EServiceType serviceType,
			string methodName,
			string data,
			UnityAction<string> responseHandler,
			UnityAction<byte[]> responseHandlerBinary)
		{
			try
			{
				MegaChakaConnection.Connected = true;
				MegaChakaConnection.OfflineMode = false;

				// 1. PutItem (Pazara eşya koyma - Alış veya Satış)
				if (methodName == "PutItem")
				{
					VaynaSocialPlugin.Log?.LogInfo("[VaynaSocial] MegaChakaConnection.CallMethod direct PutItem: " + data);
					TradingEngine.HandlePutItem(data, responseHandler);
					return false;
				}

				// 2. GetPlacedItems (Oyuncunun pazara koyduğu kendi aktif emirleri)
				if (methodName == "GetPlacedItems")
				{
					TradingEngine.HandleGetPlacedItems(responseHandler);
					return false;
				}

				// 3. CancelItem (Aktif emri iptal etme ve eşyayı geri alma)
				if (methodName == "CancelItem")
				{
					TradingEngine.HandleCancelItem(data, responseHandler);
					return false;
				}

				// 4. BuyItem (Pazardan eşya satın alma)
				if (methodName == "BuyItem")
				{
					TradingEngine.HandleBuyItem(data, responseHandler);
					return false;
				}

				// 5. SearchItem (Pazar vitrinini listeleme / filtreleme)
				if (methodName == "SearchItem")
				{
					TradingEngine.HandleSearchItem(data, responseHandler);
					return false;
				}

				// 6. RequestConversionRate (Kur ve vergi parametreleri)
				if (methodName == "RequestConversionRate")
				{
					TradingEngine.EnsureBarterAuctionItem();
					if (responseHandler != null)
					{
						responseHandler("1;10;1.0;10;1000;Token Item;10;0.05;0.00;1.0");
					}
					return false;
				}

				// 7. GetProfile (Profil ve istif envanter verisi)
				if (methodName == "GetProfile")
				{
					string localProfile = CacheManager.GetCachedProfileJson();
					if (!string.IsNullOrEmpty(localProfile) && localProfile.Contains("PlayerId"))
					{
						if (responseHandler != null)
						{
							responseHandler(localProfile);
						}
						return false;
					}
				}

				// 8. Temel Steam ve Sistem Kontrolleri (Kuyruk tıkanmasını önler)
				if (methodName == "CheckSteamBinding")
				{
					if (responseHandler != null)
					{
						responseHandler("+");
					}
					return false;
				}

				if (methodName == "Echo")
				{
					if (responseHandler != null)
					{
						responseHandler("{\"Result\":\"OK\"}");
					}
					return false;
				}

				if (methodName == "GetForbiddenItems" || methodName == "GetNamedEntities")
				{
					if (responseHandler != null)
					{
						responseHandler("[]");
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogError("Patch_MegaChakaConnection_CallMethod prefix error: " + ex.Message);
			}

			// Diğer istekler için normal kuyruğa devam et
			return true;
		}
	}
}
