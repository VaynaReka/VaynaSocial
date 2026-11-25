// Mod yapimcisi: Vayna Reka
// Bagimlilik: Bu mod VaynaCore.dll'ye baglidir (BepInEx/plugins icinde bulunmali).

using System;

using System.Collections;

using System.Collections.Generic;

using System.Reflection;

using System.Text.RegularExpressions;

using BepInEx.Logging;

using HarmonyLib;

using LitJson;

using UnityEngine;

using UnityEngine.Events;

using UnityEngine.Networking;

using System.Text;



namespace VaynaSocial

{

	// Token: 0x02000006 RID: 6

	public class ChatEngine : MonoBehaviour

	{

		// Token: 0x17000004 RID: 4

		// (get) Token: 0x06000022 RID: 34 RVA: 0x00003175 File Offset: 0x00001375

		// (set) Token: 0x06000023 RID: 35 RVA: 0x0000317C File Offset: 0x0000137C

		public static ChatEngine Instance { get; private set; }



		// Token: 0x06000024 RID: 36 RVA: 0x00003184 File Offset: 0x00001384

		public static void AddRecentMessage(SimpleChat.ChatMessage msg)

		{

			if (msg == null)

			{

				return;

			}

			object obj = ChatEngine.messageLock;

			lock (obj)

			{

				ChatEngine.RecentMessages.Add(msg);

				if (ChatEngine.RecentMessages.Count > 150)

				{

					ChatEngine.RecentMessages.RemoveAt(0);

				}

			}

		}



		// Token: 0x06000025 RID: 37 RVA: 0x000031E8 File Offset: 0x000013E8

		private void Awake()

		{

			ChatEngine.Instance = this;

		}



		// Token: 0x06000026 RID: 38 RVA: 0x000031F0 File Offset: 0x000013F0

		private void Start()

		{

			base.StartCoroutine(this.ChatPollLoop());

		}



		// Token: 0x06000027 RID: 39 RVA: 0x000031FF File Offset: 0x000013FF

		public void SendChatMessage(string messageText, string channel = "Global", long timeTicks = 0)

		{

			base.StartCoroutine(this.SendChatMessageCoroutine(messageText, channel, timeTicks));

		}



		// Token: 0x06000028 RID: 40 RVA: 0x00003210 File Offset: 0x00001410

		public static void RecordRecentlySentLocal(string message)

		{

			if (string.IsNullOrEmpty(message))

			{

				return;

			}

			List<KeyValuePair<string, float>> obj = ChatEngine.recentlySentLocal;

			lock (obj)

			{

				ChatEngine.recentlySentLocal.Add(new KeyValuePair<string, float>(message.Trim(), Time.realtimeSinceStartup));

				if (ChatEngine.recentlySentLocal.Count > 100)

				{

					ChatEngine.recentlySentLocal.RemoveAt(0);

				}

			}

		}



		// Token: 0x06000029 RID: 41 RVA: 0x00003288 File Offset: 0x00001488

		public static bool IsRecentlySentLocal(string message)

		{

			if (string.IsNullOrEmpty(message))

			{

				return false;

			}

			string b = message.Trim();

			float realtimeSinceStartup = Time.realtimeSinceStartup;

			List<KeyValuePair<string, float>> obj = ChatEngine.recentlySentLocal;

			lock (obj)

			{

				ChatEngine.recentlySentLocal.RemoveAll(x => realtimeSinceStartup - x.Value > 30f);

				return ChatEngine.recentlySentLocal.Exists(x => string.Equals(x.Key, b, StringComparison.OrdinalIgnoreCase));

			}

		}



		// Token: 0x0600002A RID: 42 RVA: 0x00003354 File Offset: 0x00001554

		public void HandleOutgoingMessage(string rawData)

		{

			if (string.IsNullOrWhiteSpace(rawData))

			{

				return;

			}

			string text = rawData;

			string text2 = "Global";

			if (rawData.TrimStart(Array.Empty<char>()).StartsWith("{"))

			{

				try

				{

					JsonData jsonData = JsonMapper.ToObject(rawData);

					if (jsonData != null && jsonData.IsObject)

					{

						JsonData jsonData2 = jsonData;

						if (((IDictionary)jsonData2).Contains("Message") && jsonData["Message"] != null)

						{

							text = jsonData["Message"].ToString();

						}

						if (((IDictionary)jsonData2).Contains("To") && jsonData["To"] != null)

						{

							text2 = jsonData["To"].ToString();

						}

					}

				}

				catch

				{

					Match match = Regex.Match(rawData, "\"Message\"\\s*:\\s*\"(.*?)(?<!\\\\)\"");

					Match match2 = Regex.Match(rawData, "\"To\"\\s*:\\s*\"(.*?)(?<!\\\\)\"");

					if (match.Success)

					{

						text = Regex.Unescape(match.Groups[1].Value);

					}

					if (match2.Success)

					{

						text2 = match2.Groups[1].Value;

					}

				}

			}

			if (text.TrimStart(Array.Empty<char>()).StartsWith("{") && text.Contains("\"Message\""))

			{

				try

				{

					Match match3 = Regex.Match(text, "\"Message\"\\s*:\\s*\"(.*?)(?<!\\\\)\"");

					if (match3.Success)

					{

						text = Regex.Unescape(match3.Groups[1].Value);

					}

				}

				catch

				{

				}

			}

			if (string.IsNullOrEmpty(text2))

			{

				text2 = "Global";

			}

			this.SendChatMessage(text, text2);

		}



		// Token: 0x0600002B RID: 43 RVA: 0x000034D8 File Offset: 0x000016D8

		private IEnumerator SendChatMessageCoroutine(string messageText, string channel, long timeTicks = 0)

		{

			if (string.IsNullOrWhiteSpace(messageText))

			{

				yield break;

			}

			string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();

			MegaChakaConnection.PlayerId = playerDisplayName;

			long ticks = (timeTicks > 0) ? timeTicks : DateTime.UtcNow.Ticks;



			string safeMsg = EscapeJsonString(messageText);

			string safeFrom = EscapeJsonString(playerDisplayName);

			string safeTo = EscapeJsonString(channel);

			string jsonPayload = "{\"From\":\"" + safeFrom + "\",\"To\":\"" + safeTo + "\",\"Message\":\"" + safeMsg + "\",\"Type\":0,\"Time\":" + ticks.ToString() + "}";



			string text2 = ModConfig.SocialServerUrl.Value.TrimEnd('/') + "/MailChatService/PostChatMessage/";

			using (UnityWebRequest unityWebRequest = new UnityWebRequest(text2, "POST"))

			{

				unityWebRequest.timeout = 5;


				byte[] bytes = Encoding.UTF8.GetBytes(jsonPayload);

				unityWebRequest.uploadHandler = new UploadHandlerRaw(bytes);

				unityWebRequest.uploadHandler.contentType = "application/json";

				unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

				unityWebRequest.SetRequestHeader("Content-Type", "application/json");

				unityWebRequest.SetRequestHeader("User-Agent", "TLIML-VaynaSocial/3.8.5");

				unityWebRequest.SetRequestHeader("OnlineTicket", VaynaSocialPlugin.CachedAccountKey);

				yield return unityWebRequest.SendWebRequest();

				if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)

				{

					ManualLogSource log = VaynaSocialPlugin.Log;

					if (log != null)

					{

						log.LogWarning("[ChatEngine] Mesaj sunucuya iletilemedi: " + unityWebRequest.responseCode + " - " + unityWebRequest.error);

					}

				}

				else

				{

					ManualLogSource log2 = VaynaSocialPlugin.Log;

					if (log2 != null)

					{

						log2.LogInfo("[ChatEngine] Mesaj sunucuya basariyla iletildi: " + messageText);

					}

					this.PollOnce();

				}

			}

			yield break;

		}



		private static string EscapeJsonString(string s)

		{

			if (string.IsNullOrEmpty(s)) return "";

			return s.Replace("\\", "\\\\")

			        .Replace("\"", "\\\"")

			        .Replace("\r", "\\r")

			        .Replace("\n", "\\n")

			        .Replace("\t", "\\t");

		}



		// Token: 0x0600002C RID: 44 RVA: 0x000034F5 File Offset: 0x000016F5

		public void PollOnce()

		{

			if (!this.isPolling)

			{

				base.StartCoroutine(this.PollChatOnce());

			}

		}



		// Token: 0x0600002D RID: 45 RVA: 0x0000350C File Offset: 0x0000170C

		private IEnumerator ChatPollLoop()

		{

			for (;;)

			{

				float waitTime = this.IsChatWindowVisible() ? Mathf.Max(2.5f, ModConfig.ChatPollInterval.Value) : 15f;

				yield return new WaitForSecondsRealtime(waitTime);



				if (ModConfig.EnableSocial.Value && MegaChakaConnection.Connected)

				{

					yield return base.StartCoroutine(this.PollChatOnce());

				}

			}

		}



		// Token: 0x0600002E RID: 46 RVA: 0x0000351C File Offset: 0x0000171C

		private bool IsChatWindowVisible()

		{

			try

			{

				if (VaynaChatWindow.IsOpen)

				{

					return true;

				}

				if (SimpleChat.Instance != null && SimpleChat.Instance.FullChat != null && SimpleChat.Instance.FullChat.activeInHierarchy)

				{

					return true;

				}

				if (SocialWindow.Instance != null && SocialWindow.Instance.Root != null && SocialWindow.Instance.Root.activeInHierarchy)

				{

					return true;

				}

			}

			catch

			{

			}

			return false;

		}



		// Token: 0x0600002F RID: 47 RVA: 0x000035B4 File Offset: 0x000017B4

		private IEnumerator PollChatOnce()

		{

			if (this.isPolling)

			{

				yield break;

			}

			this.isPolling = true;

			string url = ModConfig.SocialServerUrl.Value.TrimEnd('/') + "/MailChatService/GetChatSync/?afterId=" + VaynaSocialPlugin.lastSyncedChatId.ToString();

			using (UnityWebRequest www = UnityWebRequest.Get(url))

			{

				www.timeout = 5;


				www.SetRequestHeader("User-Agent", "TLIML-VaynaSocial/3.8.5");

				www.SetRequestHeader("OnlineTicket", VaynaSocialPlugin.CachedAccountKey);

				yield return www.SendWebRequest();

				this.isPolling = false;

				if (!www.isNetworkError && !www.isHttpError && www.downloadHandler != null)

				{

					this.ProcessChatResponse(www.downloadHandler.text);

				}

			}

			yield break;

		}



		// Token: 0x06000030 RID: 48 RVA: 0x000035C4 File Offset: 0x000017C4

		private void ProcessChatResponse(string jsonText)

		{

			if (string.IsNullOrEmpty(jsonText) || jsonText == "[]")

			{

				return;

			}

			try

			{

				JsonData jsonData = JsonMapper.ToObject(jsonText);

				if (jsonData != null && jsonData.IsArray)

				{

					for (int i = 0; i < jsonData.Count; i++)

					{

						JsonData jsonData2 = jsonData[i];

						if (jsonData2 != null)

						{

							IDictionary dictionary = jsonData2;

							int num = 0;

							if (dictionary != null && dictionary.Contains("Id") && jsonData2["Id"] != null)

							{

								int.TryParse(jsonData2["Id"].ToString(), out num);

							}

							if (num > VaynaSocialPlugin.lastSyncedChatId)

							{

								VaynaSocialPlugin.lastSyncedChatId = num;

							}

							string text = (dictionary != null && dictionary.Contains("Message") && jsonData2["Message"] != null) ? jsonData2["Message"].ToString() : "";

							string text2 = (dictionary != null && dictionary.Contains("From") && jsonData2["From"] != null) ? jsonData2["From"].ToString() : "";

							string text3 = (dictionary != null && dictionary.Contains("To") && jsonData2["To"] != null) ? jsonData2["To"].ToString() : "Global";

							long num2 = 0L;

							if (dictionary != null && dictionary.Contains("Time") && jsonData2["Time"] != null)

							{

								long.TryParse(jsonData2["Time"].ToString(), out num2);

							}

							if (text.TrimStart(Array.Empty<char>()).StartsWith("{") && text.Contains("\"Message\""))

							{

								try

								{

									Match match = Regex.Match(text, "\"Message\"\\s*:\\s*\"(.*?)(?<!\\\\)\"");

									Match match2 = Regex.Match(text, "\"To\"\\s*:\\s*\"(.*?)(?<!\\\\)\"");

									if (match.Success)

									{

										text = Regex.Unescape(match.Groups[1].Value);

									}

									if (match2.Success && (text3 == "Global" || string.IsNullOrEmpty(text3)))

									{

										text3 = match2.Groups[1].Value;

									}

								}

								catch

								{

								}

							}

							if (string.Equals(text3, "Kabile", StringComparison.OrdinalIgnoreCase))

							{

								text3 = "Global";

							}



							string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();

							if (string.IsNullOrEmpty(text2))
							{
								text2 = "Player";
							}


							if (!string.IsNullOrEmpty(text3) && !text3.Equals("Global", StringComparison.OrdinalIgnoreCase) && !text3.Equals("All", StringComparison.OrdinalIgnoreCase))

							{

								bool flag = !string.IsNullOrEmpty(playerDisplayName) && string.Equals(text2, playerDisplayName, StringComparison.OrdinalIgnoreCase);

								bool flag2 = !string.IsNullOrEmpty(playerDisplayName) && string.Equals(text3, playerDisplayName, StringComparison.OrdinalIgnoreCase);

								if (!flag && !flag2)

								{

									goto IL_508;

								}

							}

							string item = (num > 0) ? ("id:" + num.ToString()) : string.Format("{0}:{1}:{2}", text2, num2, text);

							if (!this.processedChatKeys.Contains(item))

							{

								this.processedChatKeys.Add(item);

								if (this.processedChatKeys.Count > 600)

								{

									this.processedChatKeys.Clear();

								}

								SimpleChat.ChatMessage chatMessage = new SimpleChat.ChatMessage

								{

									From = text2,

									To = text3,

									Message = text,

									Time = ((num2 > 0L) ? ((num2 < 10000000000000L) ? (num2 * 10000L + 621355968000000000L) : num2) : DateTime.UtcNow.Ticks),

									Type = 0

								};

								bool isLocalEcho = !string.IsNullOrEmpty(playerDisplayName) && 

								                   string.Equals(text2, playerDisplayName, StringComparison.OrdinalIgnoreCase) && 

								                   ChatEngine.IsRecentlySentLocal(text);

								if (!isLocalEcho)

								{

									ChatEngine.AddRecentMessage(chatMessage);

									if (SimpleChat.Instance != null)

									{

										try

										{

											Patch_SimpleChat_Update.EnsureChatInitialized(SimpleChat.Instance);

											SimpleChat.Instance.MessageReceived(chatMessage, false);

										}

										catch

										{

										}

									}

								}

							}

						}

						IL_508:;

					}

				}

			}

			catch (Exception ex)

			{

				if (ModConfig.DebugLog.Value)

				{

					ManualLogSource log = VaynaSocialPlugin.Log;

					if (log != null)

					{

						log.LogWarning("ProcessChatResponse hatası: " + ex.Message);

					}

				}

			}

		}



		// Token: 0x0400001E RID: 30

		public static readonly List<SimpleChat.ChatMessage> RecentMessages = new List<SimpleChat.ChatMessage>();



		// Token: 0x0400001F RID: 31

		private static readonly object messageLock = new object();



		// Token: 0x04000020 RID: 32

		private readonly HashSet<string> processedChatKeys = new HashSet<string>();



		// Token: 0x04000021 RID: 33

		private bool isPolling;



		// Token: 0x04000022 RID: 34

		private static readonly List<KeyValuePair<string, float>> recentlySentLocal = new List<KeyValuePair<string, float>>();

	}

}

