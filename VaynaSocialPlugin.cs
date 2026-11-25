using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VaynaSocial
{
	// Token: 0x02000002 RID: 2
	[BepInPlugin("com.vaynareka.vaynasocial", "TLIML Vayna Social & Mail System", "3.8.0")]
	[BepInProcess("This Land Is My Land.exe")]
	public sealed class VaynaSocialPlugin : BaseUnityPlugin
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		// (set) Token: 0x06000002 RID: 2 RVA: 0x00002057 File Offset: 0x00000257
		public static VaynaSocialPlugin Instance { get; private set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000003 RID: 3 RVA: 0x0000205F File Offset: 0x0000025F
		// (set) Token: 0x06000004 RID: 4 RVA: 0x00002066 File Offset: 0x00000266
		public static ManualLogSource Log { get; private set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000005 RID: 5 RVA: 0x0000206E File Offset: 0x0000026E
		// (set) Token: 0x06000006 RID: 6 RVA: 0x00002076 File Offset: 0x00000276
		public Harmony HarmonyInstance { get; private set; }

		public static void Dbg(string where, Exception ex)
		{
			try
			{
				if (Log != null && ModConfig.DebugLog != null && ModConfig.DebugLog.Value)
					Log.LogWarning("[VaynaSocial][DBG] " + where + ": " + ex);
			}
			catch { }
		}

		public static void Dbg(string msg)
		{
			try
			{
				if (Log != null && ModConfig.DebugLog != null && ModConfig.DebugLog.Value)
					Log.LogInfo("[VaynaSocial][DBG] " + msg);
			}
			catch { }
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002080 File Offset: 0x00000280
		private void Awake()
		{
			VaynaSocialPlugin.Instance = this;
			VaynaSocialPlugin.Log = base.Logger;
			ModConfig.Init(base.Config);
			VaynaSocialPlugin.Log.LogInfo("=========================================================");
			VaynaSocialPlugin.Log.LogInfo("TLIML Vayna Social & Mail System v3.8.5 Yüklendi!");
			VaynaSocialPlugin.Log.LogInfo("Mod Mimarı & Geliştirici: Vayna Reka");
			VaynaSocialPlugin.Log.LogInfo("Sosyal Ağ: " + ModConfig.SocialServerUrl.Value + " (Kabile Ağı)");
			VaynaSocialPlugin.Log.LogInfo("Özel Posta Ağı: " + ModConfig.MailServerUrl.Value);
			VaynaSocialPlugin.Log.LogInfo("=========================================================");
			VaynaSocialPlugin.CachedAccountKey = VaynaSocialPlugin.GetOrCreateAccountKey();
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			MegaChakaConnection.PlayerId = VaynaSocialPlugin.GetPlayerDisplayName();
			VaynaSocialPlugin.SetConnectionOnlineTicket(VaynaSocialPlugin.CachedAccountKey);
			try
			{
				this.HarmonyInstance = new Harmony("com.vaynareka.vaynasocial");
				this.HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
				VaynaSocialPlugin.Log.LogInfo("[VaynaSocial] Tüm ağ, pazar, etkinlik, liderlik ve posta yamaları başarıyla uygulandı.");
			}
			catch (Exception ex)
			{
				ManualLogSource log = VaynaSocialPlugin.Log;
				string str = "[VaynaSocial] Harmony yama hatası: ";
				Exception ex2 = ex;
				log.LogError(str + ((ex2 != null) ? ex2.ToString() : null));
			}
			base.gameObject.AddComponent<MailEngine>();
			base.gameObject.AddComponent<ChatEngine>();
			base.gameObject.AddComponent<CounterUploader>();
			base.gameObject.AddComponent<VaynaModsWindow>();
			base.gameObject.AddComponent<VaynaChatWindow>();
			base.gameObject.AddComponent<CorpseCleanupManager>();
			RivalDataIntegrator.RunAutomaticMigration();
			VaynaSocialPlugin.EnsureProfileLoaded();
			TradingEngine.Initialize();
			VaynaSocialPlugin.Log.LogInfo("[VaynaSocial] Kabile sosyal ağı devrede. İyi oyunlar Şef!");
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002218 File Offset: 0x00000418
		private void Start()
		{
			VaynaChatWindow.Close();
			VaynaModsWindow.Close();
			VaynaSocialPlugin.EnsureChatReady();
			VaynaModsWindow.LoadCustomModsFromDisk();
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002228 File Offset: 0x00000428
		private void Update()
		{
			if (PlayerActions.Instance == null || PlayerActions.Instance.entity == null)
			{
				if (VaynaChatWindow.IsOpen) VaynaChatWindow.Close();
				return;
			}

			if (Input.GetKeyDown(KeyCode.F10))
			{
				VaynaModsWindow.IsOpen = !VaynaModsWindow.IsOpen;
			}
			if (Input.GetKeyDown(KeyCode.F9))
			{
				if (VaynaChatWindow.IsOpen)
				{
					VaynaChatWindow.Close();
				}
				else
				{
					VaynaChatWindow.Open();
				}
			}
			if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !VaynaChatWindow.IsOpen && !this.HasActiveInputField())
			{
				VaynaSocialPlugin.OpenChat();
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (VaynaChatWindow.IsOpen)
				{
					VaynaChatWindow.Close();
				}
				if (SimpleChat.Instance != null && SimpleChat.Instance.FullChat != null && SimpleChat.Instance.FullChat.activeSelf)
				{
					try
					{
						SimpleChat.Instance.CloseChat();
					}
					catch
					{
					}
				}
			}
			if (Time.time - this.lastChatCheck > 4f)
			{
				this.lastChatCheck = Time.time;
				try
				{
					MegaChakaConnection.Connected = true;
					MegaChakaConnection.OfflineMode = false;
					MegaChakaConnection.PlayerId = VaynaSocialPlugin.GetPlayerDisplayName();
					VaynaSocialPlugin.EnsureChatReady();
				}
				catch
				{
				}
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002350 File Offset: 0x00000550
		public static SimpleChat EnsureChatReady()
		{
			try
			{
				MegaChakaConnection.Connected = true;
				MegaChakaConnection.OfflineMode = false;
				if (SimpleChat.Instance == null)
				{
					GameObject gameObject = Resources.Load<GameObject>("UI/Chat Panel");
					if (gameObject != null)
					{
						UnityEngine.Object.DontDestroyOnLoad(UnityEngine.Object.Instantiate<GameObject>(gameObject));
						ManualLogSource log = VaynaSocialPlugin.Log;
						if (log != null)
						{
							log.LogInfo("[VaynaSocial] UI/Chat Panel başarıyla oluşturuldu ve DontDestroyOnLoad yapıldı.");
						}
					}
					else
					{
						ManualLogSource log2 = VaynaSocialPlugin.Log;
						if (log2 != null)
						{
							log2.LogWarning("[VaynaSocial] UI/Chat Panel Resources içinde bulunamadı.");
						}
					}
				}
				if (SimpleChat.Instance != null)
				{
					Patch_SimpleChat_Update.EnsureChatInitialized(SimpleChat.Instance);
					return SimpleChat.Instance;
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log3 = VaynaSocialPlugin.Log;
				if (log3 != null)
				{
					log3.LogWarning("[VaynaSocial] EnsureChatReady hatası: " + ex.Message);
				}
			}
			return SimpleChat.Instance;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000241C File Offset: 0x0000061C
		public static void OpenChat()
		{
			SimpleChat simpleChat = VaynaSocialPlugin.EnsureChatReady();
			bool flag = false;
			if (simpleChat != null)
			{
				try
				{
					simpleChat.gameObject.SetActive(true);
					if (simpleChat.Canvas != null)
					{
						simpleChat.Canvas.enabled = true;
						simpleChat.Canvas.gameObject.SetActive(true);
					}
					simpleChat.ShowChat();
					if (simpleChat.FullChat != null)
					{
						simpleChat.FullChat.SetActive(true);
						flag = simpleChat.FullChat.activeSelf;
					}
					if (simpleChat.CurrentInput != null)
					{
						simpleChat.CurrentInput.gameObject.SetActive(true);
						simpleChat.CurrentInput.Select();
						simpleChat.CurrentInput.ActivateInputField();
					}
					Cursor.visible = true;
					Cursor.lockState = CursorLockMode.None;
					HUD.MouseNeeded = true;
				}
				catch (Exception ex)
				{
					ManualLogSource log = VaynaSocialPlugin.Log;
					if (log != null)
					{
						log.LogWarning("[VaynaSocial] Native chat açma hatası: " + ex.Message);
					}
				}
			}
			if (!flag || simpleChat == null)
			{
				VaynaChatWindow.Open();
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002530 File Offset: 0x00000730
		private bool HasActiveInputField()
		{
			try
			{
				EventSystem current = EventSystem.current;
				GameObject gameObject = (current != null) ? current.currentSelectedGameObject : null;
				if (gameObject != null && (gameObject.GetComponent<InputField>() != null || gameObject.name.IndexOf("input", StringComparison.OrdinalIgnoreCase) >= 0))
				{
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002598 File Offset: 0x00000798
		internal static string GetOrCreateAccountKey()
		{
			string text = PlayerPrefs.GetString("TLIML.Social.AccountKey.v1", string.Empty).Trim();
			if (VaynaSocialPlugin.AccountKeyPattern.IsMatch(text))
			{
				return text.ToLowerInvariant();
			}
			string text2 = PlayerPrefs.GetString("Vayna.Social.AccountKey.v1", string.Empty).Trim();
			if (VaynaSocialPlugin.AccountKeyPattern.IsMatch(text2))
			{
				return text2.ToLowerInvariant();
			}
			string text3 = "1e97d1ba3314b9ee76c36db6459663c65ddb873f6be458472499eff89073cee6";
			PlayerPrefs.SetString("TLIML.Social.AccountKey.v1", text3);
			PlayerPrefs.SetString("Vayna.Social.AccountKey.v1", text3);
			PlayerPrefs.Save();
			return text3;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x0000261C File Offset: 0x0000081C
		internal static string GetPlayerDisplayName()
		{
			if (ModConfig.CustomChatDisplayName != null && !string.IsNullOrWhiteSpace(ModConfig.CustomChatDisplayName.Value))
			{
				string text = ModConfig.CustomChatDisplayName.Value.Trim();
				if (!string.Equals(text, "HowlingWolf", StringComparison.OrdinalIgnoreCase) && !string.Equals(text, "Howling Wolf", StringComparison.OrdinalIgnoreCase))
				{
					return text;
				}
			}
			try
			{
				Type type = AccessTools.TypeByName("Steamworks.SteamFriends");
				MethodInfo methodInfo = (type == null) ? null : AccessTools.Method(type, "GetPersonaName", null, null);
				string text2 = (methodInfo == null) ? null : (methodInfo.Invoke(null, null) as string);
				if (!string.IsNullOrWhiteSpace(text2) && !string.Equals(text2, "HowlingWolf", StringComparison.OrdinalIgnoreCase) && !string.Equals(text2, "Howling Wolf", StringComparison.OrdinalIgnoreCase))
				{
					return text2.Trim();
				}
			}
			catch
			{
			}
			try
			{
				if (PlayerActions.Instance != null && PlayerActions.Instance.entity != null && !string.IsNullOrEmpty(PlayerActions.Instance.entity.Name))
				{
					string text3 = PlayerActions.Instance.entity.Name.Trim();
					if (!string.Equals(text3, "HowlingWolf", StringComparison.OrdinalIgnoreCase) && !string.Equals(text3, "Howling Wolf", StringComparison.OrdinalIgnoreCase))
					{
						return text3;
					}
				}
			}
			catch {}
			return "Vayna Reka";
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002870 File Offset: 0x00000A70
		internal static void SetConnectionOnlineTicket(string key)
		{
			try
			{
				FieldInfo fieldInfo = AccessTools.Field(typeof(MegaChakaConnection), "handlingComponent");
				MegaChakaConnection megaChakaConnection = ((fieldInfo != null) ? fieldInfo.GetValue(null) : null) as MegaChakaConnection;
				if (megaChakaConnection != null)
				{
					megaChakaConnection.OnlineTicket = key;
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000028CC File Offset: 0x00000ACC
		internal static void TriggerTcpHandler(string methodName, string payload)
		{
			try
			{
				FieldInfo fieldInfo = AccessTools.Field(typeof(TcpConnection), "tcpHandlers");
				Dictionary<string, UnityAction<string>> dictionary = ((fieldInfo != null) ? fieldInfo.GetValue(null) : null) as Dictionary<string, UnityAction<string>>;
				UnityAction<string> unityAction;
				if (dictionary != null && dictionary.TryGetValue(methodName, out unityAction) && unityAction != null)
				{
					unityAction(payload);
				}
			}
			catch (Exception ex)
			{
				if (ModConfig.DebugLog.Value)
				{
					ManualLogSource log = VaynaSocialPlugin.Log;
					if (log != null)
					{
						log.LogWarning("TriggerTcpHandler exception (" + methodName + "): " + ex.Message);
					}
				}
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002960 File Offset: 0x00000B60
		public static void ConsolidateProfile(SocialPlayerProfile profile)
		{
			if (profile == null)
			{
				return;
			}
			MegaChakaConnection.PlayerId = VaynaSocialPlugin.GetPlayerDisplayName();
			if (profile.Items == null || profile.Items.Count <= 1)
			{
				return;
			}
			List<SocialPlayerProfile.ItemDescription> list = new List<SocialPlayerProfile.ItemDescription>();
			Dictionary<string, SocialPlayerProfile.ItemDescription> dictionary = new Dictionary<string, SocialPlayerProfile.ItemDescription>(StringComparer.OrdinalIgnoreCase);
			foreach (SocialPlayerProfile.ItemDescription itemDescription in profile.Items)
			{
				if (itemDescription != null)
				{
					if (itemDescription.IsTimeBased || (itemDescription.AdditionalItems != null && itemDescription.AdditionalItems.Count > 0) || !string.IsNullOrEmpty(itemDescription.BundleName))
					{
						list.Add(itemDescription);
					}
					else
					{
						string text = itemDescription.Prefab;
						if (string.IsNullOrEmpty(text) && itemDescription.ItemPrefab != null)
						{
							text = itemDescription.ItemPrefab.name;
						}
						SocialPlayerProfile.ItemDescription itemDescription2;
						if (string.IsNullOrEmpty(text))
						{
							list.Add(itemDescription);
						}
						else if (dictionary.TryGetValue(text, out itemDescription2))
						{
							itemDescription2.Stack += itemDescription.Stack;
						}
						else
						{
							dictionary[text] = itemDescription;
							list.Add(itemDescription);
						}
					}
				}
			}
			profile.Items = list;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002A9C File Offset: 0x00000C9C
		public static void ConsolidatePlayerProfile(FeaturesSocialControl.PlayerProfile profile)
		{
			if (profile == null)
			{
				return;
			}
			MegaChakaConnection.PlayerId = VaynaSocialPlugin.GetPlayerDisplayName();
			if (profile.Items == null || profile.Items.Count <= 1)
			{
				return;
			}
			List<FeaturesSocialControl.PlayerProfile.ItemDescription> list = new List<FeaturesSocialControl.PlayerProfile.ItemDescription>();
			Dictionary<string, FeaturesSocialControl.PlayerProfile.ItemDescription> dictionary = new Dictionary<string, FeaturesSocialControl.PlayerProfile.ItemDescription>(StringComparer.OrdinalIgnoreCase);
			foreach (FeaturesSocialControl.PlayerProfile.ItemDescription itemDescription in profile.Items)
			{
				if (itemDescription != null)
				{
					if (itemDescription.IsTimeBased)
					{
						list.Add(itemDescription);
					}
					else
					{
						string text = itemDescription.Prefab;
						if (string.IsNullOrEmpty(text) && itemDescription.ItemPrefab != null)
						{
							text = itemDescription.ItemPrefab.name;
						}
						FeaturesSocialControl.PlayerProfile.ItemDescription itemDescription2;
						if (string.IsNullOrEmpty(text))
						{
							list.Add(itemDescription);
						}
						else if (dictionary.TryGetValue(text, out itemDescription2))
						{
							itemDescription2.Stack += itemDescription.Stack;
						}
						else
						{
							dictionary[text] = itemDescription;
							list.Add(itemDescription);
						}
					}
				}
			}
			profile.Items = list;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002BB4 File Offset: 0x00000DB4
		public static void EnsureProfileLoaded()
		{
			try
			{
				if (SocialManager.Profile == null || SocialManager.Profile.SP <= 0 || SocialManager.Profile.Items == null || SocialManager.Profile.Items.Count == 0)
				{
					string cachedProfileJson = CacheManager.GetCachedProfileJson();
					if (!string.IsNullOrEmpty(cachedProfileJson))
					{
						SocialManager.Profile = JsonUtility.FromJson<SocialPlayerProfile>(cachedProfileJson);
						if (SocialManager.Profile != null)
						{
							MegaChakaConnection.PlayerId = VaynaSocialPlugin.GetPlayerDisplayName();
							VaynaSocialPlugin.ConsolidateProfile(SocialManager.Profile);
							TradingEngine.EnsureBarterAuctionItem();
							if (SocialManager.Profile.Items != null && ItemsDB.Instance != null)
							{
								foreach (SocialPlayerProfile.ItemDescription itemDescription in SocialManager.Profile.Items)
								{
									if (itemDescription != null && !string.IsNullOrEmpty(itemDescription.Prefab))
									{
										itemDescription.ItemPrefab = ItemsDB.Instance.FindItemByPrefabName(itemDescription.Prefab, true);
										if (itemDescription.AdditionalItems != null)
										{
											foreach (SocialPlayerProfile.ItemDescription itemDescription2 in itemDescription.AdditionalItems)
											{
												if (itemDescription2 != null && !string.IsNullOrEmpty(itemDescription2.Prefab))
												{
													itemDescription2.ItemPrefab = ItemsDB.Instance.FindItemByPrefabName(itemDescription2.Prefab, true);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002CA8 File Offset: 0x00000EA8
		public static void ShowBannerNotification(string text, float duration = 5f)
		{
			if (VaynaSocialPlugin.Instance != null)
			{
				VaynaSocialPlugin.Instance.activeNotificationText = text;
				VaynaSocialPlugin.Instance.notificationEndTime = Time.time + duration;
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002CD4 File Offset: 0x00000ED4
		private void OnGUI()
		{
			if (!string.IsNullOrEmpty(this.activeNotificationText) && Time.time < this.notificationEndTime)
			{
				GUIStyle guistyle = new GUIStyle(GUI.skin.box);
				guistyle.alignment = TextAnchor.MiddleCenter;
				guistyle.fontSize = 15;
				guistyle.normal.textColor = Color.white;
				guistyle.fontStyle = FontStyle.Bold;
				float num = 640f;
				float height = 48f;
				float x = ((float)Screen.width - num) / 2f;
				float y = 45f;
				Color color = GUI.color;
				GUI.color = new Color(0.12f, 0.12f, 0.14f, 0.92f);
				GUI.Box(new Rect(x, y, num, height), GUIContent.none);
				GUI.color = color;
				GUI.Label(new Rect(x, y, num, height), this.activeNotificationText, guistyle);
			}
		}

		// Token: 0x04000001 RID: 1
		public const string PluginGUID = "com.vaynareka.vaynasocial";

		// Token: 0x04000002 RID: 2
		public const string PluginName = "TLIML Vayna Social & Mail System";

		// Token: 0x04000003 RID: 3
		public const string PluginVersion = "3.8.5";

		// Token: 0x04000004 RID: 4
		public const string Author = "Vayna Reka";

		// Token: 0x04000008 RID: 8
		private const string PrimaryAccountKeyPref = "TLIML.Social.AccountKey.v1";

		// Token: 0x04000009 RID: 9
		private const string SecondaryAccountKeyPref = "Vayna.Social.AccountKey.v1";

		// Token: 0x0400000A RID: 10
		public const string AuthenticVaynaAccountKey = "1e97d1ba3314b9ee76c36db6459663c65ddb873f6be458472499eff89073cee6";

		// Token: 0x0400000B RID: 11
		private static readonly Regex AccountKeyPattern = new Regex("^[0-9a-f]{64}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		// Token: 0x0400000C RID: 12
		internal static string CachedAccountKey = "1e97d1ba3314b9ee76c36db6459663c65ddb873f6be458472499eff89073cee6";

		// Token: 0x0400000D RID: 13
		internal static int lastSyncedChatId = 0;

		// Token: 0x0400000E RID: 14
		private float lastChatCheck;

		// Token: 0x0400000F RID: 15
		private string activeNotificationText = "";

		// Token: 0x04000010 RID: 16
		private float notificationEndTime;
	}
}
