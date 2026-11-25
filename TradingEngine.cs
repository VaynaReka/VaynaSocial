using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace VaynaSocial
{
	public static class TradingEngine
	{
		public static List<SocialBarterInfo.PutItemDesc> PlacedOffers = new List<SocialBarterInfo.PutItemDesc>();
		private static bool isInitialized = false;

		public static string PlacedItemsFilePath
		{
			get
			{
				try
				{
					return Path.Combine(CacheManager.EffectivePluginPath, "cached_placed_items.json");
				}
				catch
				{
					return @"C:\Program Files (x86)\Steam\steamapps\common\This Land Is My Land\BepInEx\plugins\cached_placed_items.json";
				}
			}
		}

		public static void Initialize()
		{
			if (isInitialized && PlacedOffers.Count > 0)
			{
				EnsureBarterPlacedItems();
				return;
			}

			try
			{
				PlacedOffers.Clear();
				string path = PlacedItemsFilePath;
				if (File.Exists(path))
				{
					string rawJson = File.ReadAllText(path, Encoding.UTF8);
					rawJson = CacheManager.CleanJson(rawJson);
					if (!string.IsNullOrEmpty(rawJson) && rawJson.Length > 2 && rawJson.StartsWith("["))
					{
						// ParadoxNotion veya eski $type artiklarini temizle
						string cleanJson = Regex.Replace(rawJson, @"""\$type""\s*:\s*""[^""]*"",?\s*", "");
						
						SocialBarterInfo.PutItemDesc[] loaded = null;
						try
						{
							loaded = BasicExtensions.FromJson<SocialBarterInfo.PutItemDesc[]>(cleanJson);
						}
						catch (Exception ex)
						{
							VaynaSocialPlugin.Log?.LogWarning("TradingEngine JSON parse fallback: " + ex.Message);
						}

						if (loaded != null && loaded.Length > 0)
						{
							foreach (var item in loaded)
							{
								if (item != null)
								{
									ValidateOffer(item);
									PlacedOffers.Add(item);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("TradingEngine Initialize error: " + ex.Message);
			}

			isInitialized = true;
			EnsureBarterPlacedItems();
		}

		private static void ValidateOffer(SocialBarterInfo.PutItemDesc item)
		{
			if (item == null) return;
			if (string.IsNullOrEmpty(item.GUID)) item.GUID = "auc_" + Guid.NewGuid().ToString("N").Substring(0, 8);
			if (string.IsNullOrEmpty(item.PlayerId)) item.PlayerId = VaynaSocialPlugin.GetPlayerDisplayName();
			if (item.Item == null) item.Item = new SocialBarterInfo.PutItemDesc.ItemAmountDescription { Prefab = "Token Item", Stack = 1 };
			if (item.Stack <= 0) item.Stack = 1;
			if (item.ExchangeTo == null || item.ExchangeTo.Length == 0)
			{
				item.ExchangeTo = new SocialBarterInfo.PutItemDesc.ItemAmountDescription[]
				{
					new SocialBarterInfo.PutItemDesc.ItemAmountDescription { Prefab = "Token Item", Stack = 1 }
				};
			}
			if (item.ExchangedItems == null)
			{
				item.ExchangedItems = new List<SocialBarterInfo.PutItemDesc.ItemAmountDescription>();
			}
		}

		public static void EnsureBarterAuctionItem()
		{
			try
			{
				if (ItemsDB.Instance == null) return;
				ItemBase tokenItem = ItemsDB.Instance.FindItemByPrefabName("Token Item", true) 
				                  ?? ItemsDB.Instance.FindItemByPrefabName("token item", true);
				if (tokenItem == null) return;

				if (SocialManager.Barter != null)
				{
					if (SocialManager.Barter.AuctionItem == null)
					{
						SocialManager.Barter.AuctionItem = tokenItem;
					}
					if (SocialManager.Barter.MaxOffers <= 0) SocialManager.Barter.MaxOffers = 10;
					if (SocialManager.Barter.SellTax <= 0f) SocialManager.Barter.SellTax = 0.05f;
					SocialManager.Barter.SetupFee = 0f;
					if (SocialManager.Barter.AuctionMinimalBuyingPriceMultiplier <= 0f) SocialManager.Barter.AuctionMinimalBuyingPriceMultiplier = 1.0f;
				}

				if (FeaturesSocialControl.Barter != null)
				{
					if (FeaturesSocialControl.Barter.AuctionItem == null)
					{
						FeaturesSocialControl.Barter.AuctionItem = tokenItem;
					}
					if (FeaturesSocialControl.Barter.MaxOffers <= 0) FeaturesSocialControl.Barter.MaxOffers = 10;
					if (FeaturesSocialControl.Barter.SellTax <= 0f) FeaturesSocialControl.Barter.SellTax = 0.05f;
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("EnsureBarterAuctionItem error: " + ex.Message);
			}
		}

		public static void EnsureBarterPlacedItems()
		{
			try
			{
				EnsureBarterAuctionItem();
				if (SocialManager.Barter != null)
				{
					SocialManager.Barter.MaxOffers = 10;
					SocialManager.Barter.PlacedItems = PlacedOffers != null ? PlacedOffers.ToArray() : new SocialBarterInfo.PutItemDesc[0];
				}
				if (FeaturesSocialControl.Barter != null)
				{
					FeaturesSocialControl.Barter.MaxOffers = 10;
					var fList = new List<FeaturesSocialControl.SocialBarterDescription.PutItemDesc>();
					if (PlacedOffers != null)
					{
						foreach (var o in PlacedOffers)
						{
							if (o == null) continue;
							var desc = new FeaturesSocialControl.SocialBarterDescription.PutItemDesc
							{
								GUID = o.GUID,
								PlayerId = o.PlayerId,
								Buying = o.Buying,
								Stack = o.Stack,
								Item = new FeaturesSocialControl.SocialBarterDescription.PutItemDesc.ItemAmountDescription
								{
									Prefab = o.Item != null ? o.Item.Prefab : "Token Item",
									Stack = o.Item != null ? o.Item.Stack : 1
								}
							};
							if (o.ExchangeTo != null && o.ExchangeTo.Length > 0)
							{
								desc.ExchangeTo = new FeaturesSocialControl.SocialBarterDescription.PutItemDesc.ItemAmountDescription[o.ExchangeTo.Length];
								for (int i = 0; i < o.ExchangeTo.Length; i++)
								{
									desc.ExchangeTo[i] = new FeaturesSocialControl.SocialBarterDescription.PutItemDesc.ItemAmountDescription
									{
										Prefab = o.ExchangeTo[i].Prefab,
										Stack = o.ExchangeTo[i].Stack
									};
								}
							}
							fList.Add(desc);
						}
					}
					FeaturesSocialControl.Barter.PlacedItems = fList.ToArray();
				}
			}
			catch {}
		}

		public static void SavePlacedOffers()
		{
			try
			{
				EnsureBarterPlacedItems();
				string json = GetPlacedItemsJson();
				File.WriteAllText(PlacedItemsFilePath, json, new UTF8Encoding(false));
				CacheManager.SaveCachedPlacedItemsJson(json);
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("TradingEngine SavePlacedOffers error: " + ex.Message);
			}
		}

		public static string GetPlacedItemsJson()
		{
			if (PlacedOffers == null || PlacedOffers.Count == 0)
			{
				return "[]";
			}

			try
			{
				// ParadoxNotion BasicExtensions.ToJson uzerinden tertemiz serialize et
				string json = BasicExtensions.ToJson(PlacedOffers.ToArray());
				if (!string.IsNullOrEmpty(json) && json.StartsWith("["))
				{
					// Temizle ve dondur
					return CacheManager.CleanJson(json);
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("TradingEngine ToJson error: " + ex.Message);
			}

			// Manuel temiz JSON uretici (tam guvence)
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			for (int i = 0; i < PlacedOffers.Count; i++)
			{
				var o = PlacedOffers[i];
				if (o == null) continue;
				ValidateOffer(o);
				if (sb.Length > 1) sb.Append(",");
				sb.Append("{");
				sb.AppendFormat("\"GUID\":\"{0}\",", o.GUID);
				sb.AppendFormat("\"PlayerId\":\"{0}\",", o.PlayerId ?? VaynaSocialPlugin.GetPlayerDisplayName());
				sb.AppendFormat("\"Buying\":{0},", o.Buying ? "true" : "false");
				sb.AppendFormat("\"Stack\":{0},", o.Stack);
				sb.AppendFormat("\"Item\":{{\"Prefab\":\"{0}\",\"Stack\":{1}}},", o.Item.Prefab, o.Item.Stack);
				sb.Append("\"ExchangeTo\":[");
				for (int j = 0; j < o.ExchangeTo.Length; j++)
				{
					if (j > 0) sb.Append(",");
					sb.AppendFormat("{{\"Prefab\":\"{0}\",\"Stack\":{1}}}", o.ExchangeTo[j].Prefab, o.ExchangeTo[j].Stack);
				}
				sb.Append("],\"ExchangedItems\":[]");
				sb.Append("}");
			}
			sb.Append("]");
			return sb.ToString();
		}

		public static void HandlePutItem(string data, UnityAction<string> responseHandler)
		{
			try
			{
				Initialize();

				string prefab = null;
				int perStack = 1;
				int totalStacks = 1;
				int price = 1;
				string currencyPrefab = "Token Item";
				bool buying = false;

				// 1. Tip guvenli JSON deserialize
				try
				{
					var putDesc = BasicExtensions.FromJson<FeaturesSocialControl.SocialBarterDescription.PutItemDesc>(data);
					if (putDesc != null)
					{
						if (putDesc.Item != null)
						{
							prefab = putDesc.Item.Prefab;
							perStack = putDesc.Item.Stack;
						}
						totalStacks = putDesc.Stack;
						buying = putDesc.Buying;
						if (putDesc.ExchangeTo != null && putDesc.ExchangeTo.Length > 0 && putDesc.ExchangeTo[0] != null)
						{
							currencyPrefab = putDesc.ExchangeTo[0].Prefab;
							price = putDesc.ExchangeTo[0].Stack;
						}
					}
				}
				catch
				{
				}

				// 2. Fallback: Regex ayrıştırma
				if (string.IsNullOrEmpty(prefab))
				{
					Match mItem = Regex.Match(data, @"""Item""\s*:\s*\{[^}]*""Prefab""\s*:\s*""([^""]+)""[^}]*""Stack""\s*:\s*(\d+)");
					if (mItem.Success)
					{
						prefab = mItem.Groups[1].Value;
						int.TryParse(mItem.Groups[2].Value, out perStack);
					}
					else
					{
						Match mP = Regex.Match(data, @"""Prefab""\s*:\s*""([^""]+)""");
						if (mP.Success) prefab = mP.Groups[1].Value;
					}

					Match mTopStack = Regex.Match(data, @"\}\s*,\s*""Stack""\s*:\s*(\d+)");
					if (mTopStack.Success)
					{
						int.TryParse(mTopStack.Groups[1].Value, out totalStacks);
					}
					else
					{
						Match mAnyStack = Regex.Match(data, @"""Stack""\s*:\s*(\d+)");
						if (mAnyStack.Success) int.TryParse(mAnyStack.Groups[1].Value, out totalStacks);
					}

					Match mExchange = Regex.Match(data, @"""ExchangeTo""\s*:\s*\[\s*\{[^}]*""Prefab""\s*:\s*""([^""]+)""[^}]*""Stack""\s*:\s*(\d+)");
					if (mExchange.Success)
					{
						currencyPrefab = mExchange.Groups[1].Value;
						int.TryParse(mExchange.Groups[2].Value, out price);
					}

					Match mBuying = Regex.Match(data, @"""Buying""\s*:\s*(true|false)", RegexOptions.IgnoreCase);
					if (mBuying.Success) bool.TryParse(mBuying.Groups[1].Value, out buying);
				}

				perStack = Math.Max(1, perStack);
				totalStacks = Math.Max(1, totalStacks);
				int totalDeduct = perStack * totalStacks;
				price = Math.Max(1, price);
				if (string.IsNullOrEmpty(currencyPrefab)) currencyPrefab = "Token Item";

				VaynaSocialPlugin.Log?.LogInfo(string.Format("[VaynaSocial] HandlePutItem: Prefab={0}, PerStack={1}, Stacks={2}, Total={3}, Price={4}, Buying={5}",
					prefab, perStack, totalStacks, totalDeduct, price, buying));

				// 3. İstiften eksiltme (Satış modundaysa eşyayı eksilt, Alış modundaysa parayı eksilt)
				if (!buying && !string.IsNullOrEmpty(prefab))
				{
					// a) cached_profile.json dosyasından kalıcı olarak düşür
					bool fileDeducted = CacheManager.DeductItemFromCachedProfile(prefab, totalDeduct);
					VaynaSocialPlugin.Log?.LogInfo(string.Format("[VaynaSocial] cached_profile.json eksiltme sonucu ({0} x{1}): {2}", prefab, totalDeduct, fileDeducted));

					// b) Canlı SocialManager.Profile.Items belleğinden düşür
					if (SocialManager.Profile != null && SocialManager.Profile.Items != null)
					{
						var item = SocialManager.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, prefab, StringComparison.OrdinalIgnoreCase) && !i.IsTimeBased);
						if (item != null)
						{
							item.Stack -= totalDeduct;
							if (item.Stack <= 0) SocialManager.Profile.Items.Remove(item);
						}
					}

					// c) Canlı FeaturesSocialControl.Profile.Items belleğinden düşür
					if (FeaturesSocialControl.Profile != null && FeaturesSocialControl.Profile.Items != null)
					{
						var item = FeaturesSocialControl.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, prefab, StringComparison.OrdinalIgnoreCase) && !i.IsTimeBased);
						if (item != null)
						{
							item.Stack -= totalDeduct;
							if (item.Stack <= 0) FeaturesSocialControl.Profile.Items.Remove(item);
						}
					}

					// d) Bekleyen eşyalardan düşür (kamp dinlenmesi öncesi)
					if (WorldState.Instance != null && WorldState.Instance.PendingSocialItems != null)
					{
						var item = WorldState.Instance.PendingSocialItems.Find(i => i != null && i.Item != null && string.Equals(i.Item.name, prefab, StringComparison.OrdinalIgnoreCase));
						if (item != null)
						{
							item.Count -= totalDeduct;
							if (item.Count <= 0) WorldState.Instance.PendingSocialItems.Remove(item);
						}
					}
				}
				else if (buying)
				{
					// Alış emrinde jeton rezerve et
					int totalTokens = price * totalStacks;
					CacheManager.DeductItemFromCachedProfile("Token Item", totalTokens);
					if (SocialManager.Profile != null && SocialManager.Profile.Items != null)
					{
						var item = SocialManager.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, "Token Item", StringComparison.OrdinalIgnoreCase));
						if (item != null)
						{
							item.Stack -= totalTokens;
							if (item.Stack <= 0) SocialManager.Profile.Items.Remove(item);
						}
					}
				}

				// 4. Teklifi oluştur ve PlacedOffers listesine ekle
				string guid = "auc_" + Guid.NewGuid().ToString("N").Substring(0, 8);
				var offer = new SocialBarterInfo.PutItemDesc
				{
					GUID = guid,
					PlayerId = VaynaSocialPlugin.GetPlayerDisplayName(),
					Buying = buying,
					Stack = totalStacks,
					Item = new SocialBarterInfo.PutItemDesc.ItemAmountDescription
					{
						Prefab = prefab,
						Stack = perStack
					},
					ExchangeTo = new SocialBarterInfo.PutItemDesc.ItemAmountDescription[]
					{
						new SocialBarterInfo.PutItemDesc.ItemAmountDescription
						{
							Prefab = currencyPrefab,
							Stack = price
						}
					},
					ExchangedItems = new List<SocialBarterInfo.PutItemDesc.ItemAmountDescription>()
				};

				PlacedOffers.Insert(0, offer);
				SavePlacedOffers();

				// 5. Oyun içi bildirim ve arayüz tetikleme
				try
				{
					string actionText = buying ? "Alış Emri Verildi" : "Pazara Konuldu";
					if (InformationalScreen.Instance != null)
					{
						InformationalScreen.Instance.AddMessage(
							string.Format("<color=#55FF55>{0}: {1} x{2} (Birim: {3} {4})</color>", actionText, prefab, totalStacks, price, currencyPrefab),
							InformationalScreen_InGame.EMessageSize.Large, 3.5f, 0.5f, false);
					}
				}
				catch {}

				if (responseHandler != null)
				{
					responseHandler("{\"Result\":\"OK\",\"GUID\":\"" + guid + "\"}");
				}

				// 6. Profil güncellendi olaylarını tetikle
				try
				{
					SocialManager.ProfileUpdated?.Invoke();
					FeaturesSocialControl.ProfileUpdated?.Invoke();
					if (SocialManager.Barter != null)
					{
						SocialManager.Barter.PlacedItemsUpdated?.Invoke();
					}
					if (FeaturesSocialControl.Barter != null)
					{
						FeaturesSocialControl.Barter.PlacedItemsUpdated?.Invoke();
					}
				}
				catch {}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogError("HandlePutItem kritik hata: " + ex.ToString());
				if (responseHandler != null)
				{
					responseHandler("{\"Result\":\"OK\",\"GUID\":\"auc_" + Guid.NewGuid().ToString("N").Substring(0, 8) + "\"}");
				}
			}
		}

		public static void HandleGetPlacedItems(UnityAction<string> responseHandler)
		{
			try
			{
				Initialize();
				string json = GetPlacedItemsJson();
				EnsureBarterPlacedItems();
				if (responseHandler != null)
				{
					responseHandler(json);
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogError("HandleGetPlacedItems hata: " + ex.ToString());
				if (responseHandler != null)
				{
					responseHandler("[]");
				}
			}
		}

		public static void HandleCancelItem(string guid, UnityAction<string> responseHandler)
		{
			try
			{
				Initialize();
				string cleanGuid = (guid ?? "").Trim().Trim('"');
				var offer = PlacedOffers.Find(o => o != null && string.Equals(o.GUID, cleanGuid, StringComparison.OrdinalIgnoreCase));
				if (offer != null)
				{
					PlacedOffers.Remove(offer);
					SavePlacedOffers();

					if (!offer.Buying && offer.Item != null)
					{
						int refundCount = Math.Max(1, offer.Item.Stack) * Math.Max(1, offer.Stack);
						string refundPrefab = offer.Item.Prefab;

						CacheManager.AddItemToCachedProfile(refundPrefab, refundCount);

						if (SocialManager.Profile != null && SocialManager.Profile.Items != null)
						{
							var pItem = SocialManager.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, refundPrefab, StringComparison.OrdinalIgnoreCase) && !i.IsTimeBased);
							if (pItem != null) pItem.Stack += refundCount;
							else
							{
								var newItem = new SocialPlayerProfile.ItemDescription
								{
									Prefab = refundPrefab,
									Stack = refundCount,
									ItemPrefab = ItemsDB.Instance != null ? ItemsDB.Instance.FindItemByPrefabName(refundPrefab, true) : null
								};
								SocialManager.Profile.Items.Add(newItem);
							}
						}

						if (FeaturesSocialControl.Profile != null && FeaturesSocialControl.Profile.Items != null)
						{
							var pItem = FeaturesSocialControl.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, refundPrefab, StringComparison.OrdinalIgnoreCase) && !i.IsTimeBased);
							if (pItem != null) pItem.Stack += refundCount;
							else
							{
								var newItem = new FeaturesSocialControl.PlayerProfile.ItemDescription
								{
									Prefab = refundPrefab,
									Stack = refundCount,
									ItemPrefab = ItemsDB.Instance != null ? ItemsDB.Instance.FindItemByPrefabName(refundPrefab, true) : null
								};
								FeaturesSocialControl.Profile.Items.Add(newItem);
							}
						}

						try
						{
							if (InformationalScreen.Instance != null)
							{
								InformationalScreen.Instance.AddMessage(
									string.Format("<color=#FFFF55>Satış İptal Edildi: {0} x{1} istife iade edildi.</color>", refundPrefab, refundCount),
									InformationalScreen_InGame.EMessageSize.Large, 3.5f, 0.5f, false);
							}
						}
						catch {}
					}
					else if (offer.Buying)
					{
						int price = (offer.ExchangeTo != null && offer.ExchangeTo.Length > 0 && offer.ExchangeTo[0] != null) ? offer.ExchangeTo[0].Stack : 1;
						int refundTokens = price * Math.Max(1, offer.Stack);

						CacheManager.AddItemToCachedProfile("Token Item", refundTokens);

						if (SocialManager.Profile != null && SocialManager.Profile.Items != null)
						{
							var pItem = SocialManager.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, "Token Item", StringComparison.OrdinalIgnoreCase));
							if (pItem != null) pItem.Stack += refundTokens;
						}

						try
						{
							if (InformationalScreen.Instance != null)
							{
								InformationalScreen.Instance.AddMessage(
									string.Format("<color=#FFFF55>Alış İptal Edildi: {0} Ticaret Jetonu iade edildi.</color>", refundTokens),
									InformationalScreen_InGame.EMessageSize.Large, 3.5f, 0.5f, false);
							}
						}
						catch {}
					}

					try
					{
						SocialManager.ProfileUpdated?.Invoke();
						FeaturesSocialControl.ProfileUpdated?.Invoke();
						if (SocialManager.Barter != null)
						{
							SocialManager.Barter.PlacedItemsUpdated?.Invoke();
						}
					}
					catch {}
				}

				if (responseHandler != null)
				{
					responseHandler("OK");
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogError("HandleCancelItem hata: " + ex.ToString());
				if (responseHandler != null)
				{
					responseHandler("OK");
				}
			}
		}

		public static void HandleBuyItem(string data, UnityAction<string> responseHandler)
		{
			try
			{
				// Format: "GUID;stacks"
				string[] parts = (data ?? "").Split(';');
				string guid = parts.Length > 0 ? parts[0].Trim() : "";
				int stacks = 1;
				if (parts.Length > 1) int.TryParse(parts[1], out stacks);

				var offer = PlacedOffers.Find(o => o != null && string.Equals(o.GUID, guid, StringComparison.OrdinalIgnoreCase));
				if (offer != null)
				{
					int pricePerStack = (offer.ExchangeTo != null && offer.ExchangeTo.Length > 0) ? offer.ExchangeTo[0].Stack : 1;
					int totalCost = pricePerStack * stacks;
					int totalReceived = offer.Item.Stack * stacks;
					string boughtPrefab = offer.Item.Prefab;

					// Jeton düş
					CacheManager.DeductItemFromCachedProfile("Token Item", totalCost);
					// Eşya ekle
					CacheManager.AddItemToCachedProfile(boughtPrefab, totalReceived);

					offer.Stack -= stacks;
					if (offer.Stack <= 0)
					{
						PlacedOffers.Remove(offer);
					}
					SavePlacedOffers();

					try
					{
						if (InformationalScreen.Instance != null)
						{
							InformationalScreen.Instance.AddMessage(
								string.Format("<color=#55FF55>Satın Alındı: {0} x{1} (-{2} Jeton)</color>", boughtPrefab, totalReceived, totalCost),
								InformationalScreen_InGame.EMessageSize.Large, 3.5f, 0.5f, false);
						}
					}
					catch {}

					try
					{
						SocialManager.ProfileUpdated?.Invoke();
						FeaturesSocialControl.ProfileUpdated?.Invoke();
						if (SocialManager.Barter != null)
						{
							SocialManager.Barter.PlacedItemsUpdated?.Invoke();
						}
					}
					catch {}
				}

				if (responseHandler != null)
				{
					responseHandler("OK");
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogError("HandleBuyItem hata: " + ex.ToString());
				if (responseHandler != null) responseHandler("OK");
			}
		}

		public static void HandleSearchItem(string data, UnityAction<string> responseHandler)
		{
			try
			{
				Initialize();
				// Oyuncunun kendi tekliflerini ve pazar tekliflerini dondur
				string json = GetPlacedItemsJson();
				if (responseHandler != null)
				{
					responseHandler(json);
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogError("HandleSearchItem hata: " + ex.ToString());
				if (responseHandler != null) responseHandler("[]");
			}
		}
	}
}
