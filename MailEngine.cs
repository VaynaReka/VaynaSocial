// Mod yapimcisi: Vayna Reka
// Bagimlilik: Bu mod VaynaCore.dll'ye baglidir (BepInEx/plugins icinde bulunmali).

using UnityEngine.Networking;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.Events;

namespace VaynaSocial
{
	// Token: 0x02000007 RID: 7
	public class MailEngine : MonoBehaviour
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00003BB3 File Offset: 0x00001DB3
		// (set) Token: 0x06000034 RID: 52 RVA: 0x00003BBA File Offset: 0x00001DBA
		public static MailEngine Instance { get; private set; }

		// Token: 0x06000035 RID: 53 RVA: 0x00003BC4 File Offset: 0x00001DC4
		private void Awake()
		{
			MailEngine.Instance = this;
			try
			{
				string text = Path.Combine(Paths.ConfigPath, "VaynaSocial");
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				this.keyFile = Path.Combine(text, "keys.txt");
				this.LoadKeys();
			}
			catch
			{
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003C24 File Offset: 0x00001E24
		private void Start()
		{
			base.StartCoroutine(this.MailPollLoop());
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00003C34 File Offset: 0x00001E34
		public static bool IsMailMethod(string m)
		{
			if (m != null)
			{
				switch (m.Length)
				{
				case 10:
					if (!(m == "RemoveMail"))
					{
						return false;
					}
					break;
				case 11:
					if (!(m == "GetFullMail"))
					{
						return false;
					}
					break;
				case 12:
					if (!(m == "GetMailEvent"))
					{
						return false;
					}
					break;
				case 13:
				case 14:
					return false;
				case 15:
					if (!(m == "RedeemMailItems"))
					{
						return false;
					}
					break;
				case 16:
				{
					char c = m[0];
					if (c != 'S')
					{
						if (c != 'U')
						{
							return false;
						}
						if (!(m == "UpdateShortMails"))
						{
							return false;
						}
					}
					else if (!(m == "SendMailToPlayer"))
					{
						return false;
					}
					break;
				}
				case 17:
					if (!(m == "MailItemsRedeemed"))
					{
						return false;
					}
					break;
				default:
					return false;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00003CF8 File Offset: 0x00001EF8
		public void Enqueue(string method, string data, UnityAction<string> handler)
		{
			MailEngine.PendingMailCall item = new MailEngine.PendingMailCall
			{
				Method = method,
				Data = data,
				Handler = handler
			};
			object obj = this.queueLock;
			lock (obj)
			{
				this.queue.Enqueue(item);
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00003D5C File Offset: 0x00001F5C
		private void Update()
		{
			MailEngine.PendingMailCall pendingMailCall = null;
			object obj = this.queueLock;
			lock (obj)
			{
				if (this.queue.Count > 0)
				{
					pendingMailCall = this.queue.Dequeue();
				}
			}
			if (pendingMailCall != null)
			{
				base.StartCoroutine(this.ExecuteMailCall(pendingMailCall));
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003DC4 File Offset: 0x00001FC4
		private IEnumerator ExecuteMailCall(MailEngine.PendingMailCall call)
		{
			if (call == null) yield break;

			if (call.Method == "GetMailEvent")
			{
				if (call.Handler != null) call.Handler.Invoke(string.Empty);
				yield break;
			}
			if (call.Method == "MailItemsRedeemed")
			{
				if (call.Handler != null) call.Handler.Invoke("OK");
				yield break;
			}

			string serverUrl = ModConfig.SocialServerUrl.Value.TrimEnd('/');
			string url = string.Concat(serverUrl, "/MailChatService/", call.Method, "/");
			using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
			{
				req.timeout = 10;
				if (!string.IsNullOrEmpty(call.Data))
				{
					req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(call.Data));
					req.uploadHandler.contentType = "application/x-www-form-urlencoded";
				}
				req.downloadHandler = new DownloadHandlerBuffer();
				req.SetRequestHeader("OnlineTicket", VaynaSocialPlugin.CachedAccountKey);

				yield return req.SendWebRequest();

				string text = null;
				if (!req.isNetworkError && !req.isHttpError && req.downloadHandler != null)
				{
					text = req.downloadHandler.text;
				}

				if (!string.IsNullOrEmpty(text) && !text.Contains("###"))
				{
					if (call.Handler != null) call.Handler.Invoke(text);
					if (call.Method == "SendMailToPlayer" && text == "OK")
					{
						MailEngine.RefreshMailboxAfterSend();
					}
					yield break;
				}
			}

			string mailUrl = ModConfig.MailServerUrl.Value;
			if (!string.IsNullOrEmpty(mailUrl))
			{
				WWWForm form = new WWWForm();
				form.AddField("m", call.Method ?? string.Empty);
				form.AddField("pid", VaynaSocialPlugin.GetPlayerDisplayName());
				form.AddField("key", this.GetKeyForPlayer(VaynaSocialPlugin.GetPlayerDisplayName()));
				form.AddField("d", call.Data ?? string.Empty);
				form.AddField("v", VaynaSocialPlugin.PluginVersion);

				using (UnityWebRequest req2 = UnityWebRequest.Post(mailUrl, form))
				{
					req2.timeout = 10;
					yield return req2.SendWebRequest();

					string res = null;
					if (!req2.isNetworkError && !req2.isHttpError && req2.downloadHandler != null)
					{
						res = req2.downloadHandler.text;
					}

					if (call.Handler != null)
					{
						call.Handler.Invoke(res ?? "OK");
					}
					if (call.Method == "SendMailToPlayer" && res == "OK")
					{
						MailEngine.RefreshMailboxAfterSend();
					}
				}
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003DDC File Offset: 0x00001FDC
		public static void RefreshMailboxAfterSend()
		{
			try
			{
				MailboxWindow.NewMailReceived(MegaChakaConnection.PlayerId = VaynaSocialPlugin.GetPlayerDisplayName());
			}
			catch (Exception ex)
			{
				ManualLogSource log = VaynaSocialPlugin.Log;
				if (log != null)
				{
					log.LogWarning("RefreshMailboxAfterSend hatası: " + ex.Message);
				}
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003E30 File Offset: 0x00002030
		private IEnumerator MailPollLoop()
		{
			while (true)
			{
				yield return new WaitForSeconds(30f);
				if (!ModConfig.EnableMail.Value) continue;

				string mailUrl = ModConfig.MailServerUrl.Value;
				if (string.IsNullOrEmpty(mailUrl)) continue;

				WWWForm form = new WWWForm();
				form.AddField("m", "poll");
				form.AddField("pid", VaynaSocialPlugin.GetPlayerDisplayName());
				form.AddField("key", this.GetKeyForPlayer(VaynaSocialPlugin.GetPlayerDisplayName()));

				using (UnityWebRequest req = UnityWebRequest.Post(mailUrl, form))
				{
					req.timeout = 8;
					yield return req.SendWebRequest();

					if (!req.isNetworkError && !req.isHttpError && req.downloadHandler != null)
					{
						string text = req.downloadHandler.text;
						if (!string.IsNullOrEmpty(text))
						{
							string[] parts = text.Split(';');
							if (parts.Length >= 2 && int.TryParse(parts[0], out int unread) && unread > 0 && long.TryParse(parts[1], out long lastId))
							{
								if (lastId > this.lastNotifiedMailId)
								{
									this.lastNotifiedMailId = lastId;
									try
									{
										MailboxWindow.NewMailReceived(VaynaSocialPlugin.GetPlayerDisplayName());
										VaynaSocialPlugin.ShowBannerNotification("Yeni bir posta aldınız!", 4f);
									}
									catch { }
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003E40 File Offset: 0x00002040
		public string GetKeyForPlayer(string pid)
		{
			if (string.IsNullOrEmpty(pid))
			{
				pid = VaynaSocialPlugin.GetPlayerDisplayName();
			}
			Dictionary<string, string> obj = this.keys;
			lock (obj)
			{
				string text;
				if (this.keys.TryGetValue(pid, out text) && !string.IsNullOrEmpty(text))
				{
					return text;
				}
				}
			return VaynaSocialPlugin.CachedAccountKey;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00003ED0 File Offset: 0x000020D0
		private void LoadKeys()
		{
			this.keys.Clear();
			Dictionary<string, string> obj = this.keys;
			lock (obj)
			{
			}
			if (File.Exists(this.keyFile))
			{
				try
				{
					foreach (string text in File.ReadAllLines(this.keyFile))
					{
						int num = text.IndexOf('=');
						if (num < 0)
						{
							num = text.IndexOf('\t');
						}
						if (num > 0)
						{
							string text2 = text.Substring(0, num).Trim();
							string value = text.Substring(num + 1).Trim();
							if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(value))
							{
								obj = this.keys;
								lock (obj)
								{
									this.keys[text2] = value;
								}
							}
						}
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x04000024 RID: 36
		private readonly object queueLock = new object();

		// Token: 0x04000025 RID: 37
		private readonly Queue<MailEngine.PendingMailCall> queue = new Queue<MailEngine.PendingMailCall>();

		// Token: 0x04000026 RID: 38
		private readonly Dictionary<string, string> keys = new Dictionary<string, string>();

		// Token: 0x04000027 RID: 39
		private string keyFile;

		// Token: 0x04000028 RID: 40
		private long lastNotifiedMailId;

		// Token: 0x02000038 RID: 56
		public sealed class PendingMailCall
		{
			// Token: 0x04000063 RID: 99
			public string Method;

			// Token: 0x04000064 RID: 100
			public string Data;

			// Token: 0x04000065 RID: 101
			public UnityAction<string> Handler;
		}
	}
}
