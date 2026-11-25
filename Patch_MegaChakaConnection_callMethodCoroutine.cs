using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(MegaChakaConnection), "callMethodCoroutine")]
	internal static class Patch_MegaChakaConnection_callMethodCoroutine
	{
		[HarmonyPrefix]
		private static bool Prefix(MegaChakaConnection.EServiceType serviceType, string methodName, ref string data, ref UnityAction<string> responseHandler, UnityAction<byte[]> responseHandlerBinary, ref IEnumerator __result)
		{
			if (serviceType == MegaChakaConnection.EServiceType.MailChatService && MailEngine.IsMailMethod(methodName))
			{
				MailEngine instance = MailEngine.Instance;
				if (instance != null)
				{
					instance.Enqueue(methodName, data, responseHandler);
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "CheckSteamBinding")
			{
				UnityAction<string> unityAction = responseHandler;
				if (unityAction != null)
				{
					unityAction("+");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "GetEndPoints")
			{
				string text = ModConfig.SocialServerUrl.Value.Trim();
				if (!text.EndsWith("/"))
				{
					text += "/";
				}
				string arg = string.Join(",", Enumerable.Repeat<string>(text, 9).ToArray<string>());
				UnityAction<string> unityAction2 = responseHandler;
				if (unityAction2 != null)
				{
					unityAction2(arg);
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "GetTime")
			{
				long ticks = DateTime.UtcNow.Ticks;
				UnityAction<string> unityAction3 = responseHandler;
				if (unityAction3 != null)
				{
					unityAction3(ticks.ToString());
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "GetNamedEntities" || methodName == "GetForbiddenItems")
			{
				UnityAction<string> unityAction4 = responseHandler;
				if (unityAction4 != null)
				{
					unityAction4("[]");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "Echo")
			{
				UnityAction<string> unityAction5 = responseHandler;
				if (unityAction5 != null)
				{
					unityAction5("{\"Result\":\"OK\"}");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "NewWorldCreated" || methodName == "SetWorld")
			{
				string data2 = data;
				VaynaSocialPlugin instance2 = VaynaSocialPlugin.Instance;
				if (instance2 != null)
				{
					instance2.StartCoroutine(Patch_MegaChakaConnection_callMethodCoroutine.SendFireAndForget(serviceType, methodName, data2));
				}
				UnityAction<string> unityAction6 = responseHandler;
				if (unityAction6 != null)
				{
					unityAction6("{\"Result\":\"OK\"}");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (serviceType == MegaChakaConnection.EServiceType.LoginService && (methodName == "LoginWithSteam" || methodName == "LoginWithXsolla"))
			{
				__result = Patch_MegaChakaConnection_callMethodCoroutine.HandleLoginCoroutine(methodName, responseHandler);
				return false;
			}
			if (methodName == "TurnInSocialEvent")
			{
				try
				{
					if (SocialManager.Profile != null)
					{
						SocialManager.Profile.SP += 1500;
					}
					CacheManager.AddSPToCachedProfile(1500);
					VaynaSocialPlugin.ShowBannerNotification("Mücadele tamamlandı! Zulaya +1500 SP eklendi.", 4f);
				}
				catch
				{
				}
				UnityAction<string> unityAction7 = responseHandler;
				if (unityAction7 != null)
				{
					unityAction7("OK");
				}
				try
				{
					UnityEvent profileUpdated = SocialManager.ProfileUpdated;
					if (profileUpdated != null)
					{
						profileUpdated.Invoke();
					}
				}
				catch
				{
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "GetSocialEventsVersion")
			{
				UnityAction<string> unityAction8 = responseHandler;
				if (unityAction8 != null)
				{
					unityAction8("999999");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "GetSocialEvents")
			{
				string cachedEventsJson = CacheManager.GetCachedEventsJson();
				UnityAction<string> unityAction9 = responseHandler;
				if (unityAction9 != null)
				{
					unityAction9(cachedEventsJson);
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "GetWarpaints")
			{
				UnityAction<string> unityAction10 = responseHandler;
				if (unityAction10 != null)
				{
					unityAction10("0|[]");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "GetTcpPort")
			{
				UnityAction<string> unityAction11 = responseHandler;
				if (unityAction11 != null)
				{
					unityAction11("0");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}
			if (methodName == "GetAvailableChannels")
			{
				string arg2 = "{\"Global\":{\"SocialName\":\"Global\",\"ChannelName\":\"Global\",\"LastMessages\":[]}}";
				UnityAction<string> unityAction12 = responseHandler;
				if (unityAction12 != null)
				{
					unityAction12(arg2);
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			if (methodName == "GetProfile")
			{
				string pJson = CacheManager.GetCachedProfileJson();
				if (responseHandler != null) responseHandler(pJson);
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			if (methodName == "LeaderboardsGetInfo")
			{
				string lbJson = CacheManager.GetCachedLeaderboardsJson();
				__result = Patch_MegaChakaConnection_callMethodCoroutine.AsyncResponseCoroutine(responseHandler, lbJson);
				return false;
			}

			if (methodName == "LeaderboardsGetRange")
			{
				string cId = "HumanKills";
				if (!string.IsNullOrEmpty(data))
				{
					var m = Regex.Match(data, "counterId=([^&]+)");
					if (m.Success) cId = m.Groups[1].Value;
					else
					{
						var m2 = Regex.Match(data, "\"CounterId\"\\s*:\\s*\"([^\"]+)\"");
						if (m2.Success) cId = m2.Groups[1].Value;
					}
				}
				string rangeJson = CacheManager.GetCachedLeaderboardRangeJson(cId);
				__result = Patch_MegaChakaConnection_callMethodCoroutine.AsyncResponseCoroutine(responseHandler, rangeJson);
				return false;
			}

			// =========================================================
			// A) İSTİFTEN EŞYA ÇEKME (Profile_GetItem) - EKSİLMEYİ SAĞLA
			// =========================================================
			if (methodName == "Profile_GetItem")
			{
				try
				{
					// Format: "index;prefab;amount"
					string[] parts = (data ?? "").Split(';');
					if (parts.Length >= 3)
					{
						string prefab = parts[1];
						int amount = 1;
						int.TryParse(parts[2], out amount);

						// 1. Yerel dosyada (cached_profile.json) kalıcı olarak eksilt
						CacheManager.DeductItemFromCachedProfile(prefab, amount);

						// 2. Canlı bellekteki profillerden anında düşür
						if (SocialManager.Profile != null && SocialManager.Profile.Items != null)
						{
							var item = SocialManager.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, prefab, StringComparison.OrdinalIgnoreCase));
							if (item != null)
							{
								item.Stack -= amount;
								if (item.Stack <= 0) SocialManager.Profile.Items.Remove(item);
							}
						}
						if (FeaturesSocialControl.Profile != null && FeaturesSocialControl.Profile.Items != null)
						{
							var item = FeaturesSocialControl.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, prefab, StringComparison.OrdinalIgnoreCase));
							if (item != null)
							{
								item.Stack -= amount;
								if (item.Stack <= 0) FeaturesSocialControl.Profile.Items.Remove(item);
							}
						}

						// 3. Arka planda sunucuya bildir
						if (VaynaSocialPlugin.Instance != null)
						{
							VaynaSocialPlugin.Instance.StartCoroutine(Patch_MegaChakaConnection_callMethodCoroutine.SendFireAndForget(serviceType, methodName, data));
						}
					}
				}
				catch (Exception ex)
				{
					VaynaSocialPlugin.Log?.LogWarning("Profile_GetItem yerel eksiltme hatasi: " + ex.Message);
				}

				if (responseHandler != null)
				{
					responseHandler("OK");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			// =========================================================
			// B) İSTİFE EŞYA YATIRMA (Profile_PutItems) - EKLENMEYİ SAĞLA
			// =========================================================
			if (methodName == "Profile_PutItems")
			{
				try
				{
					// Format: "Prefab:count;Prefab2:count2;"
					if (!string.IsNullOrEmpty(data))
					{
						string[] entries = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string entry in entries)
						{
							string[] kv = entry.Split(':');
							if (kv.Length >= 2)
							{
								string prefab = kv[0];
								int count = 1;
								int.TryParse(kv[1], out count);
								CacheManager.AddItemToCachedProfile(prefab, count);

								if (SocialManager.Profile != null && SocialManager.Profile.Items != null)
								{
									var item = SocialManager.Profile.Items.Find(i => i != null && string.Equals(i.Prefab, prefab, StringComparison.OrdinalIgnoreCase));
									if (item != null) item.Stack += count;
									else SocialManager.Profile.Items.Add(new SocialPlayerProfile.ItemDescription { Prefab = prefab, Stack = count });
								}
							}
						}
						if (VaynaSocialPlugin.Instance != null)
						{
							VaynaSocialPlugin.Instance.StartCoroutine(Patch_MegaChakaConnection_callMethodCoroutine.SendFireAndForget(serviceType, methodName, data));
						}
					}
				}
				catch (Exception ex)
				{
					VaynaSocialPlugin.Log?.LogWarning("Profile_PutItems yerel ekleme hatasi: " + ex.Message);
				}

				if (responseHandler != null)
				{
					responseHandler("OK");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			// =========================================================
			// C) İSTİFTEN SP ÇEKME (Profile_GetSP)
			// =========================================================
			if (methodName == "Profile_GetSP")
			{
				int spAmount = 0;
				int.TryParse(data ?? "0", out spAmount);
				if (spAmount > 0)
				{
					CacheManager.DeductSPFromCachedProfile(spAmount);
					if (SocialManager.Profile != null)
					{
						SocialManager.Profile.SP = Math.Max(0, SocialManager.Profile.SP - spAmount);
					}
				}
				if (VaynaSocialPlugin.Instance != null)
				{
					VaynaSocialPlugin.Instance.StartCoroutine(Patch_MegaChakaConnection_callMethodCoroutine.SendFireAndForget(serviceType, methodName, data));
				}
				if (responseHandler != null)
				{
					responseHandler(data ?? "0");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			// =========================================================
			// D) PAZAR İŞLEMLERİ (PutItem, GetPlacedItems, CancelItem, BuyItem, SearchItem)
			// =========================================================
						if (methodName == "RequestConversionRate")
			{
				TradingEngine.EnsureBarterAuctionItem();
				if (responseHandler != null)
				{
					responseHandler("1;10;1.0;10;1000;Token Item;10;0.05;0.00;1.0");
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			if (methodName == "PutItem")
			{
				TradingEngine.HandlePutItem(data, responseHandler);
				if (VaynaSocialPlugin.Instance != null)
				{
					VaynaSocialPlugin.Instance.StartCoroutine(Patch_MegaChakaConnection_callMethodCoroutine.SendFireAndForget(serviceType, methodName, data));
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			if (methodName == "GetPlacedItems")
			{
				TradingEngine.HandleGetPlacedItems(responseHandler);
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			if (methodName == "CancelItem")
			{
				TradingEngine.HandleCancelItem(data, responseHandler);
				if (VaynaSocialPlugin.Instance != null)
				{
					VaynaSocialPlugin.Instance.StartCoroutine(Patch_MegaChakaConnection_callMethodCoroutine.SendFireAndForget(serviceType, methodName, data));
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			if (methodName == "BuyItem")
			{
				TradingEngine.HandleBuyItem(data, responseHandler);
				if (VaynaSocialPlugin.Instance != null)
				{
					VaynaSocialPlugin.Instance.StartCoroutine(Patch_MegaChakaConnection_callMethodCoroutine.SendFireAndForget(serviceType, methodName, data));
				}
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			if (methodName == "SearchItem")
			{
				TradingEngine.HandleSearchItem(data, responseHandler);
				__result = Patch_MegaChakaConnection_callMethodCoroutine.EmptyCoroutine();
				return false;
			}

			__result = Patch_MegaChakaConnection_callMethodCoroutine.ForwardToCloudflareCoroutine(serviceType, methodName, data, responseHandler);
			return false;
		}

		private static IEnumerator HandleLoginCoroutine(string methodName, UnityAction<string> responseHandler)
		{
			string serverUrl = ModConfig.SocialServerUrl.Value.TrimEnd('/');
			string url = string.Format("{0}/LoginService/{1}/", serverUrl, methodName);
			using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
			{
				req.timeout = 12;
				req.certificateHandler = new AcceptAnyCertificateHandler();
				req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(string.Empty));
				req.uploadHandler.contentType = "application/x-www-form-urlencoded";
				req.downloadHandler = new DownloadHandlerBuffer();
				req.SetRequestHeader("OnlineTicket", VaynaSocialPlugin.CachedAccountKey);

				yield return req.SendWebRequest();

				string text = null;
				if (!req.isNetworkError && !req.isHttpError && req.downloadHandler != null)
				{
					text = req.downloadHandler.text;
				}

				string pName = VaynaSocialPlugin.GetPlayerDisplayName();
				string ticket = VaynaSocialPlugin.CachedAccountKey;
				string port = "25565";

				if (!string.IsNullOrEmpty(text) && text.Contains(";") && !text.StartsWith("error;") && !text.StartsWith("tag_error;"))
				{
					string[] parts = text.Split(';');
					if (parts.Length >= 3)
					{
						if (!string.IsNullOrWhiteSpace(parts[0])) pName = parts[0].Trim();
						if (!string.IsNullOrWhiteSpace(parts[1])) ticket = parts[1].Trim();
						if (!string.IsNullOrWhiteSpace(parts[2])) port = parts[2].Trim();
					}
					else if (parts.Length == 2)
					{
						if (!string.IsNullOrWhiteSpace(parts[0])) ticket = parts[0].Trim();
						if (!string.IsNullOrWhiteSpace(parts[1])) port = parts[1].Trim();
					}
				}

				text = pName + ";" + ticket + ";" + port;
				VaynaSocialPlugin.SetConnectionOnlineTicket(ticket);

				if (responseHandler != null)
				{
					responseHandler.Invoke(text);
				}
			}
		}

		private static IEnumerator ForwardToCloudflareCoroutine(MegaChakaConnection.EServiceType serviceType, string methodName, string data, UnityAction<string> responseHandler)
		{
			string serverUrl = ModConfig.SocialServerUrl.Value.TrimEnd('/');
			string url = string.Concat(serverUrl, "/", serviceType.ToString(), "/", methodName);
			using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
			{
				req.timeout = 12;
				req.certificateHandler = new AcceptAnyCertificateHandler();
				if (!string.IsNullOrEmpty(data))
				{
					req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
					req.uploadHandler.contentType = "application/x-www-form-urlencoded";
				}
				req.downloadHandler = new DownloadHandlerBuffer();
				req.SetRequestHeader("OnlineTicket", VaynaSocialPlugin.CachedAccountKey);
				if (WorldState.Instance != null && !string.IsNullOrEmpty(WorldState.Instance.UID))
				{
					req.SetRequestHeader("WorldId", WorldState.Instance.UID);
				}

				yield return req.SendWebRequest();

				string response = null;
				if (!req.isNetworkError && !req.isHttpError && req.downloadHandler != null)
				{
					response = req.downloadHandler.text;
				}

				if (methodName == "GetProfile")
				{
					string localProfile = CacheManager.GetCachedProfileJson();
					if (!string.IsNullOrEmpty(localProfile) && localProfile.Contains("PlayerId"))
					{
						response = localProfile;
					}
					else if (!string.IsNullOrEmpty(response) && response.Contains("PlayerId"))
					{
						string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();
						response = Regex.Replace(response, "\"PlayerId\"\\s*:\\s*\"[^\"]*\"", "\"PlayerId\":\"" + playerDisplayName + "\"");
						CacheManager.SaveCachedProfileJson(response);
					}
					else
					{
						response = CacheManager.GetCachedProfileJson();
					}
				}
				else if (methodName == "SearchItem")
				{
					// DIKKAT: response == "[]" bos sonuc anlamina gelir, HATA DEGILDIR!
					// Yalnizca ag hatasi veya bos string olursa yerel onbellege dus
					if (string.IsNullOrWhiteSpace(response) || response.StartsWith("error"))
					{
						response = CacheManager.GetCachedAuctionJson();
					}
				}
				else if (methodName == "LeaderboardsGetInfo")
				{
					if (string.IsNullOrWhiteSpace(response) || response == "{}" || response.StartsWith("error"))
					{
						response = CacheManager.GetCachedLeaderboardsJson();
					}
				}
				else if (methodName == "LeaderboardsGetRange")
				{
					if (string.IsNullOrWhiteSpace(response) || response.Contains("\"Range\":{}") || response.StartsWith("error"))
					{
						string counterId = "HumanKills";
						if (!string.IsNullOrEmpty(data) && data.Contains("CounterId"))
						{
							Match m = Regex.Match(data, "\"CounterId\"\\s*:\\s*\"([^\"]+)\"");
							if (m.Success) counterId = m.Groups[1].Value;
						}
						response = CacheManager.GetCachedLeaderboardRangeJson(counterId);
					}
				}

				if (responseHandler != null)
				{
					responseHandler.Invoke(response ?? "{}");
				}
			}
		}

		private static IEnumerator SendFireAndForget(MegaChakaConnection.EServiceType serviceType, string methodName, string data)
		{
			string serverUrl = ModConfig.SocialServerUrl.Value.TrimEnd('/');
			string url = string.Format("{0}/{1}/{2}/", serverUrl, serviceType, methodName);
			using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
			{
				req.timeout = 10;
				req.certificateHandler = new AcceptAnyCertificateHandler();
				if (!string.IsNullOrEmpty(data))
				{
					req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
					req.uploadHandler.contentType = "application/x-www-form-urlencoded";
				}
				req.downloadHandler = new DownloadHandlerBuffer();
				req.SetRequestHeader("OnlineTicket", VaynaSocialPlugin.CachedAccountKey);
				yield return req.SendWebRequest();
			}
		}

		private static IEnumerator AsyncResponseCoroutine(UnityAction<string> responseHandler, string json)
		{
			yield return null;
			try
			{
				if (responseHandler != null)
				{
					responseHandler(json);
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("AsyncResponseCoroutine error: " + ex.Message);
			}
		}

		private static IEnumerator EmptyCoroutine()
		{
			yield break;
		}
	}
}
