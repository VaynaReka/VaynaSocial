using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using LitJson;
using UnityEngine;

namespace VaynaSocial
{
	// Token: 0x02000032 RID: 50
	public class VaynaModsWindow : MonoBehaviour
	{
		// Token: 0x0600007E RID: 126 RVA: 0x0000707C File Offset: 0x0000527C
		public static void LoadCustomModsFromDisk()
		{
			try
			{
				string text = Path.Combine(Paths.ConfigPath, "VaynaModsList.json");
				if (File.Exists(text))
				{
					string text2 = File.ReadAllText(text, Encoding.UTF8);
					if (!string.IsNullOrWhiteSpace(text2))
					{
						List<VaynaModsWindow.ModEntry> list = JsonMapper.ToObject<List<VaynaModsWindow.ModEntry>>(text2);
						if (list != null)
						{
							using (List<VaynaModsWindow.ModEntry>.Enumerator enumerator = list.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									VaynaModsWindow.ModEntry customMod = enumerator.Current;
									if (!string.IsNullOrEmpty(customMod.Id))
									{
										int num = VaynaModsWindow.AllMods.FindIndex((VaynaModsWindow.ModEntry m) => m.Id.Equals(customMod.Id, StringComparison.OrdinalIgnoreCase));
										if (num >= 0)
										{
											VaynaModsWindow.AllMods[num] = customMod;
										}
										else
										{
											VaynaModsWindow.AllMods.Add(customMod);
										}
									}
								}
							}
							ManualLogSource log = VaynaSocialPlugin.Log;
							if (log != null)
							{
								log.LogInfo(string.Format("[VaynaSocial] VaynaModsList.json dosyasından {0} mod başarıyla yüklendi/güncellendi.", list.Count));
							}
						}
					}
				}
				else
				{
					string contents = JsonMapper.ToJson(VaynaModsWindow.AllMods);
					File.WriteAllText(text, contents, Encoding.UTF8);
					ManualLogSource log2 = VaynaSocialPlugin.Log;
					if (log2 != null)
					{
						log2.LogInfo("[VaynaSocial] VaynaModsList.json şablonu oluşturuldu: " + text);
					}
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log3 = VaynaSocialPlugin.Log;
				if (log3 != null)
				{
					log3.LogWarning("[VaynaSocial] LoadCustomModsFromDisk hatası: " + ex.Message);
				}
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x0000720C File Offset: 0x0000540C
		public static void Open()
		{
			VaynaModsWindow.IsOpen = true;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00007214 File Offset: 0x00005414
		public static void Close()
		{
			VaynaModsWindow.IsOpen = false;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x0000721C File Offset: 0x0000541C
		private void OnGUI()
		{
			if (!VaynaModsWindow.IsOpen)
			{
				return;
			}
			VaynaModsWindow.InitStyles();
			GUI.color = new Color(0f, 0f, 0f, 0.72f);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), VaynaModsWindow.bgTex);
			GUI.color = Color.white;
			float num = Mathf.Min(980f, (float)Screen.width - 60f);
			float num2 = Mathf.Min(720f, (float)Screen.height - 60f);
			float num3 = ((float)Screen.width - num) / 2f;
			float num4 = ((float)Screen.height - num2) / 2f;
			GUI.Box(new Rect(num3, num4, num, num2), GUIContent.none, VaynaModsWindow.windowStyle);
			GUILayout.BeginArea(new Rect(num3 + 24f, num4 + 20f, num - 48f, 65f));
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			GUILayout.Label("VAYNA REKA - RESMİ MODLAR VE EKLENTİLER", VaynaModsWindow.titleStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("This Land Is My Land Topluluk Genişletme Paketleri | Kabile Özgürlük Ağı", VaynaModsWindow.descStyle, Array.Empty<GUILayoutOption>());
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("\ud83d\udc7e DİSCORD KABİLESİ ↗", VaynaModsWindow.buttonStyle, new GUILayoutOption[]
			{
				GUILayout.Width(190f),
				GUILayout.Height(38f)
			}))
			{
				VaynaModsWindow.OpenDiscordInvite();
			}
			GUILayout.Space(8f);
			if (GUILayout.Button("\ud83d\udcac SOHBETİ AÇ (T/F9)", VaynaModsWindow.buttonStyle, new GUILayoutOption[]
			{
				GUILayout.Width(170f),
				GUILayout.Height(38f)
			}))
			{
				VaynaModsWindow.Close();
				VaynaSocialPlugin.OpenChat();
			}

			GUILayout.Space(8f);
			if (GUILayout.Button("✕ KAPAT (ESC)", VaynaModsWindow.buttonStyle, new GUILayoutOption[]
			{
				GUILayout.Width(130f),
				GUILayout.Height(38f)
			}))
			{
				VaynaModsWindow.Close();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			VaynaModsWindow.DrawHorizontalLine(num3 + 24f, num4 + 88f, num - 48f, new Color(0.6f, 0.5f, 0.35f, 0.5f));
			float y = num4 + 96f;
			float height = num2 - 150f;
			GUILayout.BeginArea(new Rect(num3 + 24f, y, num - 48f, height));
			GUILayout.BeginHorizontal(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Label("<color=#7289DA><b>\ud83d\udc7e RESMİ DİSCORD KABİLEMİZ:</b></color> <color=#DDDDDD>Canlı kabile sohbeti, takas, kabile yardımlaşması ve mod güncellemeleri için bekliyoruz!</color>", VaynaModsWindow.descStyle, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("<b>DİSCORD'A KATIL ↗</b>", VaynaModsWindow.buttonStyle, new GUILayoutOption[]
			{
				GUILayout.Width(170f),
				GUILayout.Height(30f)
			}))
			{
				VaynaModsWindow.OpenDiscordInvite();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(8f);
			this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, false, true, Array.Empty<GUILayoutOption>());
			foreach (VaynaModsWindow.ModEntry mod in VaynaModsWindow.AllMods)
			{
				this.RenderModCard(mod, num - 76f);
				GUILayout.Space(10f);
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();
			VaynaModsWindow.DrawHorizontalLine(num3 + 24f, num4 + num2 - 50f, num - 48f, new Color(0.6f, 0.5f, 0.35f, 0.5f));
			GUILayout.BeginArea(new Rect(num3 + 24f, num4 + num2 - 42f, num - 48f, 36f));
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label("<color=#CCCCCC>Kısayollar: </color><color=#FFDD88>F10</color> Mod Menüsü | <color=#FFDD88>T / F9</color> Sohbet | <color=#7289DA>Discord Kabilemiz: </color><color=#FFFFFF><b>discord.gg/HqJRZQZN4t</b></color>", VaynaModsWindow.descStyle, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			if (!string.IsNullOrEmpty(VaynaModsWindow.toastMessage) && Time.unscaledTime - VaynaModsWindow.toastTime < 4f)
			{
				GUILayout.Label("<color=#55FF55>✓ " + VaynaModsWindow.toastMessage + "</color>", VaynaModsWindow.descStyle, Array.Empty<GUILayoutOption>());
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
			{
				VaynaModsWindow.Close();
				Event.current.Use();
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00007658 File Offset: 0x00005858
		private void RenderModCard(VaynaModsWindow.ModEntry mod, float cardWidth)
		{
			VaynaModsWindow.ModState modState = this.GetModState(mod);
			GUILayout.BeginVertical(VaynaModsWindow.cardStyle, new GUILayoutOption[]
			{
				GUILayout.Width(cardWidth)
			});
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.BeginVertical(new GUILayoutOption[]
			{
				GUILayout.Width(cardWidth - 250f)
			});
			GUILayout.Label("<b><size=15>" + mod.NameTr + "</size></b>", VaynaModsWindow.titleStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Space(2f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (mod.IsUpdated || !string.IsNullOrEmpty(mod.UpdateTag))
			{
				string str = (!string.IsNullOrEmpty(mod.UpdateTag)) ? mod.UpdateTag : "★ GÜNCELLENDİ";
				GUILayout.Label("<color=#FFD700><b>[" + str + "]</b></color>", VaynaModsWindow.badgeUpdatedStyle, new GUILayoutOption[]
				{
					GUILayout.ExpandWidth(false)
				});
				GUILayout.Space(8f);
			}
			GUILayout.Label(string.Concat(new string[]
			{
				"<color=#88BBFF>Kategori: ",
				mod.Category,
				"</color>  <color=#666666>•</color>  <color=#AAAAAA>Sürüm: ",
				mod.Version,
				"</color>"
			}), VaynaModsWindow.descStyle, new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false)
			});
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(4f);
			GUILayout.Label(mod.DescTr, VaynaModsWindow.descStyle, Array.Empty<GUILayoutOption>());
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical(new GUILayoutOption[]
			{
				GUILayout.Width(220f)
			});
			if (modState == VaynaModsWindow.ModState.Active)
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.FlexibleSpace();
				GUILayout.Label("✓ YÜKLÜ / AKTİF", VaynaModsWindow.badgeActiveStyle, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				GUILayout.Space(2f);
				if (GUILayout.Button("⏸ Pasifleştir", VaynaModsWindow.buttonDangerStyle, new GUILayoutOption[]
				{
					GUILayout.Height(26f)
				}))
				{
					this.ToggleModState(mod);
				}
			}
			else if (modState == VaynaModsWindow.ModState.Disabled)
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.FlexibleSpace();
				GUILayout.Label("⊗ PASİF (DEVRE DIŞI)", VaynaModsWindow.badgeDisabledStyle, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
				GUILayout.Space(2f);
				if (GUILayout.Button("▶ Aktifleştir", VaynaModsWindow.buttonSuccessStyle, new GUILayoutOption[]
				{
					GUILayout.Height(26f)
				}))
				{
					this.ToggleModState(mod);
				}
			}
			else
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.FlexibleSpace();
				GUILayout.Label("⬇ İNDİRİLEBİLİR", VaynaModsWindow.badgeDownloadStyle, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
			}
			GUILayout.Space(4f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("\ud83c\udf10 NexusMods", VaynaModsWindow.buttonStyle, new GUILayoutOption[]
			{
				GUILayout.Height(28f)
			}))
			{
				VaynaModsWindow.OpenModUrl(mod);
			}
			if (GUILayout.Button("Link", VaynaModsWindow.buttonStyle, new GUILayoutOption[]
			{
				GUILayout.Width(42f),
				GUILayout.Height(28f)
			}))
			{
				GUIUtility.systemCopyBuffer = (string.IsNullOrEmpty(mod.NexusUrl) ? "https://www.nexusmods.com/thislandismyland/mods/" : mod.NexusUrl);
				VaynaModsWindow.toastMessage = mod.NameTr + " linki kopyalandı!";
				VaynaModsWindow.toastTime = Time.unscaledTime;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00007994 File Offset: 0x00005B94
		private static void OpenModUrl(VaynaModsWindow.ModEntry mod)
		{
			if (mod == null)
			{
				return;
			}
			string text = string.IsNullOrEmpty(mod.NexusUrl) ? "https://www.nexusmods.com/thislandismyland/mods/" : mod.NexusUrl;
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = text,
					UseShellExecute = true
				});
				GUIUtility.systemCopyBuffer = text;
				VaynaModsWindow.toastMessage = mod.NameTr + " Nexus sayfası açıldı!";
				VaynaModsWindow.toastTime = Time.unscaledTime;
			}
			catch
			{
				try
				{
					Application.OpenURL(text);
					GUIUtility.systemCopyBuffer = text;
					VaynaModsWindow.toastMessage = mod.NameTr + " Nexus sayfası açıldı!";
					VaynaModsWindow.toastTime = Time.unscaledTime;
				}
				catch (Exception ex)
				{
					VaynaModsWindow.toastMessage = "Link açılamadı: " + ex.Message;
					VaynaModsWindow.toastTime = Time.unscaledTime;
				}
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00007A70 File Offset: 0x00005C70
		public static void OpenDiscordInvite()
		{
			string text = (ModConfig.DiscordInviteUrl != null && !string.IsNullOrEmpty(ModConfig.DiscordInviteUrl.Value)) ? ModConfig.DiscordInviteUrl.Value : "https://discord.gg/HqJRZQZN4t";
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = text,
					UseShellExecute = true
				});
				GUIUtility.systemCopyBuffer = text;
				VaynaModsWindow.toastMessage = "Discord davet linki açıldı (Panoya da kopyalandı)!";
				VaynaModsWindow.toastTime = Time.unscaledTime;
			}
			catch
			{
				try
				{
					Application.OpenURL(text);
					GUIUtility.systemCopyBuffer = text;
					VaynaModsWindow.toastMessage = "Discord davet linki açıldı!";
					VaynaModsWindow.toastTime = Time.unscaledTime;
				}
				catch (Exception ex)
				{
					VaynaModsWindow.toastMessage = "Discord linki açılamadı: " + ex.Message;
					VaynaModsWindow.toastTime = Time.unscaledTime;
				}
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00007B40 File Offset: 0x00005D40
		private VaynaModsWindow.ModState GetModState(VaynaModsWindow.ModEntry mod)
		{
			try
			{
				string pluginPath = Paths.PluginPath;
				string text = Path.Combine(pluginPath, mod.DllName);
				string path = text + ".disabled";
				string path2 = Path.Combine(Path.Combine(pluginPath, "Disabled"), mod.DllName);
				if (File.Exists(text))
				{
					return VaynaModsWindow.ModState.Active;
				}
				if (File.Exists(path) || File.Exists(path2))
				{
					return VaynaModsWindow.ModState.Disabled;
				}
				if (Chainloader.PluginInfos.ContainsKey(mod.Id))
				{
					return VaynaModsWindow.ModState.Active;
				}
			}
			catch
			{
			}
			return VaynaModsWindow.ModState.NotInstalled;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00007BD0 File Offset: 0x00005DD0
		private void ToggleModState(VaynaModsWindow.ModEntry mod)
		{
			if (mod == null || string.IsNullOrEmpty(mod.DllName))
			{
				return;
			}
			if (mod.DllName.Equals("VaynaSocial.dll", StringComparison.OrdinalIgnoreCase))
			{
				VaynaModsWindow.toastMessage = "VaynaSocial bu menüyü çalıştırdığı için oyun içinden pasifleştirilemez!";
				VaynaModsWindow.toastTime = Time.unscaledTime;
				return;
			}
			try
			{
				string pluginPath = Paths.PluginPath;
				string text = Path.Combine(pluginPath, mod.DllName);
				string text2 = text + ".disabled";
				string text3 = Path.Combine(Path.Combine(pluginPath, "Disabled"), mod.DllName);
				VaynaModsWindow.ModState modState = this.GetModState(mod);
				if (modState == VaynaModsWindow.ModState.Active)
				{
					if (File.Exists(text))
					{
						if (File.Exists(text2))
						{
							File.Delete(text2);
						}
						File.Move(text, text2);
						VaynaModsWindow.toastMessage = "⏸ " + mod.NameTr + " pasifleştirildi (.disabled yapıldı)! Oyunu yeniden başlattığınızda devre dışı kalır.";
						VaynaModsWindow.toastTime = Time.unscaledTime;
					}
				}
				else if (modState == VaynaModsWindow.ModState.Disabled)
				{
					if (File.Exists(text2))
					{
						if (File.Exists(text))
						{
							File.Delete(text);
						}
						File.Move(text2, text);
						VaynaModsWindow.toastMessage = "▶ " + mod.NameTr + " aktifleştirildi! Oyunu yeniden başlattığınızda yüklenecektir.";
						VaynaModsWindow.toastTime = Time.unscaledTime;
					}
					else if (File.Exists(text3))
					{
						if (File.Exists(text))
						{
							File.Delete(text);
						}
						File.Move(text3, text);
						VaynaModsWindow.toastMessage = "▶ " + mod.NameTr + " aktifleştirildi! Oyunu yeniden başlattığınızda yüklenecektir.";
						VaynaModsWindow.toastTime = Time.unscaledTime;
					}
				}
			}
			catch (Exception ex)
			{
				VaynaModsWindow.toastMessage = "İşlem başarısız: " + ex.Message;
				VaynaModsWindow.toastTime = Time.unscaledTime;
			}
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00007D6C File Offset: 0x00005F6C
		private static void DrawHorizontalLine(float x, float y, float width, Color color)
		{
			Color color2 = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(new Rect(x, y, width, 1.5f), VaynaModsWindow.bgTex);
			GUI.color = color2;
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00007D98 File Offset: 0x00005F98
		private static void InitStyles()
		{
			if (VaynaModsWindow.bgTex == null)
			{
				VaynaModsWindow.bgTex = new Texture2D(1, 1);
				VaynaModsWindow.bgTex.SetPixel(0, 0, Color.white);
				VaynaModsWindow.bgTex.Apply();
			}
			if (VaynaModsWindow.cardTex == null)
			{
				VaynaModsWindow.cardTex = new Texture2D(1, 1);
				VaynaModsWindow.cardTex.SetPixel(0, 0, new Color(0.12f, 0.1f, 0.08f, 0.95f));
				VaynaModsWindow.cardTex.Apply();
			}
			if (VaynaModsWindow.windowStyle == null)
			{
				VaynaModsWindow.windowStyle = new GUIStyle(GUI.skin.box);
				Texture2D texture2D = new Texture2D(1, 1);
				texture2D.SetPixel(0, 0, new Color(0.08f, 0.07f, 0.06f, 0.98f));
				texture2D.Apply();
				VaynaModsWindow.windowStyle.normal.background = texture2D;
				VaynaModsWindow.windowStyle.border = new RectOffset(4, 4, 4, 4);
			}
			if (VaynaModsWindow.cardStyle == null)
			{
				VaynaModsWindow.cardStyle = new GUIStyle();
				VaynaModsWindow.cardStyle.normal.background = VaynaModsWindow.cardTex;
				VaynaModsWindow.cardStyle.padding = new RectOffset(16, 16, 12, 12);
				VaynaModsWindow.cardStyle.margin = new RectOffset(0, 0, 4, 4);
			}
			if (VaynaModsWindow.titleStyle == null)
			{
				VaynaModsWindow.titleStyle = new GUIStyle();
				VaynaModsWindow.titleStyle.fontStyle = FontStyle.Bold;
				VaynaModsWindow.titleStyle.normal.textColor = new Color(0.96f, 0.88f, 0.72f);
				VaynaModsWindow.titleStyle.fontSize = 16;
				VaynaModsWindow.titleStyle.wordWrap = true;
			}
			if (VaynaModsWindow.descStyle == null)
			{
				VaynaModsWindow.descStyle = new GUIStyle();
				VaynaModsWindow.descStyle.normal.textColor = new Color(0.82f, 0.8f, 0.76f);
				VaynaModsWindow.descStyle.fontSize = 13;
				VaynaModsWindow.descStyle.wordWrap = true;
			}
			if (VaynaModsWindow.badgeActiveStyle == null)
			{
				VaynaModsWindow.badgeActiveStyle = new GUIStyle();
				VaynaModsWindow.badgeActiveStyle.fontStyle = FontStyle.Bold;
				VaynaModsWindow.badgeActiveStyle.normal.textColor = new Color(0.35f, 0.95f, 0.45f);
				VaynaModsWindow.badgeActiveStyle.alignment = TextAnchor.MiddleRight;
				VaynaModsWindow.badgeActiveStyle.fontSize = 13;
			}
			if (VaynaModsWindow.badgeDownloadStyle == null)
			{
				VaynaModsWindow.badgeDownloadStyle = new GUIStyle();
				VaynaModsWindow.badgeDownloadStyle.fontStyle = FontStyle.Bold;
				VaynaModsWindow.badgeDownloadStyle.normal.textColor = new Color(0.95f, 0.75f, 0.25f);
				VaynaModsWindow.badgeDownloadStyle.alignment = TextAnchor.MiddleRight;
				VaynaModsWindow.badgeDownloadStyle.fontSize = 13;
			}
			if (VaynaModsWindow.badgeDisabledStyle == null)
			{
				VaynaModsWindow.badgeDisabledStyle = new GUIStyle();
				VaynaModsWindow.badgeDisabledStyle.fontStyle = FontStyle.Bold;
				VaynaModsWindow.badgeDisabledStyle.normal.textColor = new Color(0.95f, 0.45f, 0.35f);
				VaynaModsWindow.badgeDisabledStyle.alignment = TextAnchor.MiddleRight;
				VaynaModsWindow.badgeDisabledStyle.fontSize = 13;
			}
			if (VaynaModsWindow.badgeUpdatedStyle == null)
			{
				VaynaModsWindow.badgeUpdatedStyle = new GUIStyle();
				VaynaModsWindow.badgeUpdatedStyle.fontStyle = FontStyle.Bold;
				VaynaModsWindow.badgeUpdatedStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
				VaynaModsWindow.badgeUpdatedStyle.fontSize = 12;
			}
			if (VaynaModsWindow.buttonStyle == null)
			{
				VaynaModsWindow.buttonStyle = new GUIStyle(GUI.skin.button);
				VaynaModsWindow.buttonStyle.fontStyle = FontStyle.Bold;
				VaynaModsWindow.buttonStyle.fontSize = 12;
			}
			if (VaynaModsWindow.buttonDangerStyle == null)
			{
				VaynaModsWindow.buttonDangerStyle = new GUIStyle(GUI.skin.button);
				VaynaModsWindow.buttonDangerStyle.fontStyle = FontStyle.Bold;
				VaynaModsWindow.buttonDangerStyle.fontSize = 11;
				VaynaModsWindow.buttonDangerStyle.normal.textColor = new Color(1f, 0.45f, 0.45f);
			}
			if (VaynaModsWindow.buttonSuccessStyle == null)
			{
				VaynaModsWindow.buttonSuccessStyle = new GUIStyle(GUI.skin.button);
				VaynaModsWindow.buttonSuccessStyle.fontStyle = FontStyle.Bold;
				VaynaModsWindow.buttonSuccessStyle.fontSize = 11;
				VaynaModsWindow.buttonSuccessStyle.normal.textColor = new Color(0.45f, 1f, 0.55f);
			}
		}

		// Token: 0x04000033 RID: 51
		public static bool IsOpen = false;

		// Token: 0x04000034 RID: 52
		private Vector2 scrollPos = Vector2.zero;

		// Token: 0x04000035 RID: 53
		private static Texture2D bgTex;

		// Token: 0x04000036 RID: 54
		private static Texture2D cardTex;

		// Token: 0x04000037 RID: 55
		private static GUIStyle windowStyle;

		// Token: 0x04000038 RID: 56
		private static GUIStyle cardStyle;

		// Token: 0x04000039 RID: 57
		private static GUIStyle titleStyle;

		// Token: 0x0400003A RID: 58
		private static GUIStyle descStyle;

		// Token: 0x0400003B RID: 59
		private static GUIStyle badgeActiveStyle;

		// Token: 0x0400003C RID: 60
		private static GUIStyle badgeDisabledStyle;

		// Token: 0x0400003D RID: 61
		private static GUIStyle badgeDownloadStyle;

		// Token: 0x0400003E RID: 62
		private static GUIStyle badgeUpdatedStyle;

		// Token: 0x0400003F RID: 63
		private static GUIStyle buttonStyle;

		// Token: 0x04000040 RID: 64
		private static GUIStyle buttonDangerStyle;

		// Token: 0x04000041 RID: 65
		private static GUIStyle buttonSuccessStyle;

		// Token: 0x04000042 RID: 66
		private static string toastMessage = "";

		// Token: 0x04000043 RID: 67
		private static float toastTime = 0f;

		// Token: 0x04000044 RID: 68
		public static List<VaynaModsWindow.ModEntry> AllMods = new List<VaynaModsWindow.ModEntry>
		{
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.trainrobbery",
				DllName = @"TrainRobbery.dll",
				NameTr = @"Frontier Train Robbery & Tren Sabotajı",
				NameEn = @"Frontier Train Robbery & Railroad Sabotage",
				Version = @"v1.0.0",
				Category = @"Savaş & Baskın",
				DescTr = @"Sömürgeci ikmal trenlerine pusu kurun, 30 kişilik otonom sabotaj timiyle demiryolu şantiyelerini yakın, raylara ağır patlayıcılar döşeyip lokomotifleri raydan çıkarın ve esirleri kurtarın.",
				DescEn = @"Ambush colonial supply trains with heavy demolition charges, deploy an autonomous 30-warrior sabotage squad, raid railroad worker camps, and free captured braves across the frontier.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/53",
				IsUpdated = true,
				UpdateTag = @"⭐ YENİ MOD (v1.0.0)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.fortressconquest",
				DllName = @"FortressConquest.dll",
				NameTr = @"Kale Fethi & Kuşatma Savunması",
				NameEn = @"Fortress Conquest & Siege Defense",
				Version = @"v1.0.0",
				Category = @"Savaş & Kale",
				DescTr = @"Temizlenen sömürgeci kalelerini kabilenizin kalıcı üssü yapın (F6), kutsal savaş totemi dikin, surlara savaşçı nöbetçiler yerleştirin ve büyük misilleme kuşatma dalgalarını (F10) püskürtün.",
				DescEn = @"Turn cleared colonial forts into allied sanctuaries (F6), erect sacred nocturnal war totems, station warriors on ramparts, and repel massive colonial siege waves (F10) for epic rewards.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/52",
				IsUpdated = true,
				UpdateTag = @"⭐ YENİ MOD (v1.0.0)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.vaynasocial",
				DllName = @"VaynaSocial.dll",
				NameTr = @"TLIML Vayna Social & Mail System",
				NameEn = @"TLIML Vayna Social & Mail System",
				Version = @"v3.7.0",
				Category = @"Ağ, Pazar & Sohbet",
				DescTr = @"Cloudflare tabanlı egemen kabile sosyal ağı, posta kutusu, eşya takası, canlı kabile sohbeti (T/Enter/F9), dinamik liderlik tablosu ve oyun içi Mod Yöneticisi (F10).",
				DescEn = @"Sovereign tribal social network with Cloudflare backend, mail attachments, trade barter market, live chat (T/Enter/F9), real leaderboards, and in-game Mod Manager (F10).",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/39",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v3.7.0)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.eaglecompanion",
				DllName = @"EagleCompanion.dll",
				NameTr = @"Eagle Companion & Scouting System",
				NameEn = @"Eagle Companion & Scouting System",
				Version = @"v1.1.1",
				Category = @"Yoldaşlar & Keşif",
				DescTr = @"Sadık savaş kartalı yoldaşı. Düşman kamplarını ve devriyeleri havadan keşfeder, işaretler, [Y] tuşuyla çağrılır, tüneme ve serbest uçuş mekanikleri içerir.",
				DescEn = @"Loyal tribal eagle companion. Scouts and tags enemy camps and patrols from above, called with [Y], custom perch and free-flight AI behaviors.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/43",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.1.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.campshaman",
				DllName = @"CampShaman.dll",
				NameTr = @"Tribal Shaman & Medicine Elder",
				NameEn = @"Tribal Shaman & Medicine Elder",
				Version = @"v1.7.5",
				Category = @"Kabile, Şifa & İksirler",
				DescTr = @"Kamp ateşi başında oturan Kadim Şaman. Şifa aurası, Kurt Ruhu & Ayı Dayanıklılığı kutsamaları, Gece Görüşü ve Casus İksirleri üretimi, ölümsüzlük kalkanı.",
				DescEn = @"Meditating Medicine Elder seated by campfire. Healing aura, Spirit of Wolf and Bear buffs, potion crafting (Night Vision & Infiltration Spy), full immunity.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/33",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.7.5)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.companions",
				DllName = @"Companions.dll",
				NameTr = @"Companions of the Wild",
				NameEn = @"Companions of the Wild",
				Version = @"v1.1.4",
				Category = @"Yoldaşlar & Savaş",
				DescTr = @"Kabile savaşçılarını ve yoldaşlarını yanınıza alarak seferlere çıkın. Gelişmiş emir sistemi, dinamik can yenilenmesi ve savaş taktikleri.",
				DescEn = @"Recruit and lead tribal warriors and companions into combat. Advanced order wheel, dynamic health regeneration, and battle formations.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/45",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.1.4)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.riflescopezoom",
				DllName = @"RifleScopeZoom.dll",
				NameTr = @"Rifle Scope Zoom & Dinamik Optik Dürbün",
				NameEn = @"Rifle Scope Zoom & Dynamic Optics",
				Version = @"v1.0.1",
				Category = @"Silahlar & Taktik",
				DescTr = @"Dürbünlü tüfeklerde fare tekerleğiyle kademeli 2x-12x optik yakınlaştırma ve kristal netliğinde dinamik nişangah.",
				DescEn = @"Smooth multi-level mousewheel optical zoom (2x-12x) for scoped rifles with zero performance loss.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/49",
				IsUpdated = true,
				UpdateTag = @"★ YENİ (v1.0.1) #49"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.beartaming",
				DllName = @"BearTaming.dll",
				NameTr = @"Bear Taming & Mountain Guardian",
				NameEn = @"Bear Taming & Mountain Guardian",
				Version = @"v1.0.0",
				Category = @"Yoldaşlar & Vahşi Yaşam",
				DescTr = @"Vahşi dağ ayılarını [F]/[E] ile et sunarak evcilleştirme, sadık ayı yoldaşla birlikte savaşma, çağırma ([B]), bekle/takip ([H]) ve kurt yoldaşlarla dostluk.",
				DescEn = @"Tame wild grizzly and black bears, fight alongside them in combat, name them, call with [B], stay/follow with [H], fully friendly with tamed wolves.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/47",
				IsUpdated = true,
				UpdateTag = @"★ YENİ (v1.0.0) #47"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.horsestats",
				DllName = @"HorseStats.dll",
				NameTr = @"Horse Stats & Saddlebag Capacity",
				NameEn = @"Horse Stats & Saddlebag Capacity",
				Version = @"v1.0.0",
				Category = @"Atlar & Lojistik",
				DescTr = @"Atların heybe depolama kapasitesini, hızını ve dayanıklılığını oyun dengesine uygun şekilde artıran gelişmiş at istatistik modifikasyonu.",
				DescEn = @"Extends horse saddlebag inventory capacity, speed, and endurance while preserving immersive gameplay balance.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/48",
				IsUpdated = true,
				UpdateTag = @"★ YENİ (v1.0.0) #48"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.spymod",
				DllName = @"SpyMod.dll",
				NameTr = @"Spy Remote Surveillance & Infiltration",
				NameEn = @"Spy Remote Surveillance & Infiltration",
				Version = @"v1.8.3",
				Category = @"Taktik & Keşif",
				DescTr = @"Düşman kamplarına uzaktan sızma, gizli gözetleme, alarm sabotajı ve düşman devriye rotalarını uzaktan tespit etme imkanı.",
				DescEn = @"Remote enemy outpost infiltration, covert surveillance camera mode, alarm sabotages, and scouting patrol routes safely.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/36",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.8.3)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.distinctenemymarkers",
				DllName = @"DistinctEnemyMarkers.dll",
				NameTr = @"Distinct Enemy Markers & HUD Awareness",
				NameEn = @"Distinct Enemy Markers & HUD Awareness",
				Version = @"v1.1.1",
				Category = @"Arayüz & Taktik",
				DescTr = @"Düşman tiplerini (Keskin Nişancı, Şerif, Asker, Avcı) HUD ve harita üzerinde belirgin renk ve simgelerle anında ayırt etme.",
				DescEn = @"Distinct colored tactical HUD/map markers for different enemy types (Snipers, Officers, Soldiers, Trappers) with distance indicators.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/40",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.1.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.firearrow",
				DllName = @"FireArrow.dll",
				NameTr = @"Fire and Glass Arrows Arsenal",
				NameEn = @"Fire and Glass Arrows Arsenal",
				Version = @"v1.7.8",
				Category = @"Silahlar & Cephane",
				DescTr = @"Ateşli oklar (binaları ve düşmanları yakar) ve cam uçlu parçalanan oklar ile düşman savunmalarını yerle bir eden cephane çeşitliliği.",
				DescEn = @"Explosive fire arrows (ignites structures and foes) and razor glass-tipped arrows causing severe bleeding effects.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/28",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.7.8)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.stashexpansion",
				DllName = @"StashExpansion.dll",
				NameTr = @"Stash Expansion Network Storage",
				NameEn = @"Stash Expansion Network Storage",
				Version = @"v2.1.1",
				Category = @"Envanter & Depolama",
				DescTr = @"Tüm kabile depolarının ve sandıklarının kapasitesini 500-1000 yuvaya çıkararak ganimet sınırlamalarını tamamen ortadan kaldırır.",
				DescEn = @"Expands personal camp stashes and storage containers to 500-1000 slots with instant multi-page sorting.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/27",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v2.1.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.tribalcommander",
				DllName = @"TribalCommander.dll",
				NameTr = @"Tribal Commander War Chief",
				NameEn = @"Tribal Commander War Chief",
				Version = @"v1.0.1",
				Category = @"Kabile & Savaş Yönetimi",
				DescTr = @"Tüm kabile savaşçılarına stratejik komuta verme, devriye hatları çizme, kuşatma ve savunma emirleri yağdırma yeteneği.",
				DescEn = @"Strategic war chief command interface: direct squad positioning, outpost siege orders, and unified camp defenses.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/44",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.0.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.maxskills",
				DllName = @"MaxSkills.dll",
				NameTr = @"Max Skills Instant Unlocker",
				NameEn = @"Max Skills Instant Unlocker",
				Version = @"v1.1.3",
				Category = @"Yetenekler & İlerleme",
				DescTr = @"Tüm kabile yetenek ağacını anında açar veya dilediğiniz yetenek puanını serbestçe yapılandırmanıza olanak tanır.",
				DescEn = @"Instantly unlocks all player skill tree perks or grants configurable mastery points without breaking progression.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/29",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.1.3)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.horsewarpaint",
				DllName = @"HorseWarPaint.dll",
				NameTr = @"Authentic Horse War Paint",
				NameEn = @"Authentic Horse War Paint",
				Version = @"v1.4.2",
				Category = @"Atlar & Görsel",
				DescTr = @"Tarihsel Yerli Amerikan kabile sembolleriyle donatılmış 10 farklı at savaş boyası tasarımı ve at koruma bonusları.",
				DescEn = @"10 authentic Native American horse war paint patterns and symbols with minor courage and endurance blessings.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/30",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.4.2)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.vaynacore",
				DllName = @"VaynaCore.dll",
				NameTr = @"VaynaCore SaveGuard Stabilizer",
				NameEn = @"VaynaCore SaveGuard Stabilizer",
				Version = @"v1.2.3",
				Category = @"Sistem & Kararlılık",
				DescTr = @"Kritik kayıt koruma motoru, oyun çökmelerini engelleyen bellek temizleyici ve modlar arası senkronizasyon omurgası.",
				DescEn = @"Critical savegame integrity protection, memory leak preventer, and core runtime synchronization engine.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/38",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.2.3)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.wolftaming",
				DllName = @"WolfTaming.dll",
				NameTr = @"Wolf Taming & Beast Whisperer",
				NameEn = @"Wolf Taming & Beast Whisperer",
				Version = @"v1.2.4",
				Category = @"Yoldaşlar & Vahşi Yaşam",
				DescTr = @"Vahşi kurtları evcilleştirme, sürü liderliği, kurtlarla birlikte avlanma, düşman kamplarına kurt akınları düzenleme.",
				DescEn = @"Tame wild wolves with fresh meat, command your wolf pack in battle, howl to summon, and track prey across the wilderness.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/25",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.2.4)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.burnemptycamp",
				DllName = @"BurnEmptyCamp.dll",
				NameTr = @"Burn Empty Camp Raze & Clear",
				NameEn = @"Burn Empty Camp Raze & Clear",
				Version = @"v1.1.1",
				Category = @"Kamplar & Keşif",
				DescTr = @"Düşmanlardan temizlenen terkedilmiş kampları tek tuşla yakarak haritadan temizleme ve kabileye moral/direniş kazandırma.",
				DescEn = @"Raze and set ablaze cleared enemy outposts to permanently remove them from map clutter and earn tribal resistance.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/42",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.1.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.roundup",
				DllName = @"RoundUp.dll",
				NameTr = @"Herbalist Roundup & Flora Harvester",
				NameEn = @"Herbalist Roundup & Flora Harvester",
				Version = @"v1.5.5",
				Category = @"Toplama & Doğa",
				DescTr = @"Çevredeki tüm şifalı otları, mantarları ve bitkileri tek tuşla [U] kabile çantasına toplama ve hızlı iksir hammaddesi elde etme.",
				DescEn = @"Instant nearby herb and flora harvesting with radius indicator ([U]), auto-storing into herbalist satchel.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/41",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.5.5)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.wagonmod",
				DllName = @"WagonMod.dll",
				NameTr = @"War Wagons & Camp Logistics",
				NameEn = @"War Wagons & Camp Logistics",
				Version = @"v2.5.2",
				Category = @"Araçlar & Lojistik",
				DescTr = @"Ağır yük vagonlarını kabile lojistiğinde kullanma, cephane ve erzak sevkiyatı yapma, mobil kabile karargahı kurma.",
				DescEn = @"Steerable supply and war wagons with massive cargo beds for inter-camp transport and mobile frontline supply depots.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/37",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v2.5.2)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.horsecall",
				DllName = @"HorseCallDistance.dll",
				NameTr = @"Whistle Horse Call Distance",
				NameEn = @"Whistle Horse Call Distance",
				Version = @"v1.0.1",
				Category = @"Atlar & Seslenme",
				DescTr = @"At ıslık mesafesini tüm harita çapına yayarak atınızın nerede olursa olsun anında yanınıza dört nala koşmasını sağlar.",
				DescEn = @"Unlocks horse whistle summoning distance map-wide with fast auto-pathing navigation directly to the chief.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/35",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.0.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.revealmap",
				DllName = @"RevealMap.dll",
				NameTr = @"Camp Discovery & Map Reveal",
				NameEn = @"Camp Discovery & Map Reveal",
				Version = @"v1.0.1",
				Category = @"Harita & Keşif",
				DescTr = @"Haritadaki sis perdesini kaldırır, tüm gizli madenleri, düşman karakollarını ve kutsal kabile noktalarını görünür kılar.",
				DescEn = @"Removes map fog-of-war on demand, unveiling resource deposits, secret paths, and enemy outpost strongholds.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/34",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.0.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.sharedcampinv",
				DllName = @"SharedCampInventory.dll",
				NameTr = @"Shared Camp Inventory Network",
				NameEn = @"Shared Camp Inventory Network",
				Version = @"v8.13.1",
				Category = @"Kamplar & Ağ Lojistiği",
				DescTr = @"Farklı kabile kamplarındaki tüm depoları birbirine bağlayarak her kamptan diğer kampların envanterine anında erişim sunar.",
				DescEn = @"Interlinks storage containers across all owned tribal camps into a single unified shared inventory network.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/23",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v8.13.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.bulkcrates",
				DllName = @"BulkCrates.dll",
				NameTr = @"Bulk Crates & Mass Production",
				NameEn = @"Bulk Crates & Mass Production",
				Version = @"v3.5.5",
				Category = @"Üretim & Sandıklar",
				DescTr = @"Toplu sandık üretimi, sandıkları paketleme ve kabile kampları arasında verimli büyük sevkiyatlar organize etme.",
				DescEn = @"Organized bulk crate packaging, stacking, and mass logistics distribution for tribal manufacturing lines.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/32",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v3.5.5)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.horsetaming",
				DllName = @"HorseTaming.dll",
				NameTr = @"Horse Taming & Wild Mustang Whisperer",
				NameEn = @"Horse Taming & Wild Mustang Whisperer",
				Version = @"v2.0.2",
				Category = @"Atlar & Evcilleştirme",
				DescTr = @"Vahşi mustangları ve cins atları kementle yakalama ve anında kabile bağı kurarak güvenle evcilleştirme kolaylığı.",
				DescEn = @"Enhanced wild horse lasso mechanics, smoother taming minigames, and instant stable bonding options.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/24",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v2.0.2)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.horsespeed",
				DllName = @"HorseSpeed.dll",
				NameTr = @"Horse Speed Sprint Stamina",
				NameEn = @"Horse Speed Sprint Stamina",
				Version = @"v1.0.2",
				Category = @"Atlar & Hız",
				DescTr = @"Atların dörtnal hızını ve depar dayanıklılığını artırarak harita boyunca kesintisiz hızlı seyahat imkanı tanır.",
				DescEn = @"Increases horse gallop velocity and sprint stamina duration for swift exploration across rough terrain.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/19",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.0.2)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.spyglass",
				DllName = @"SpyGlass.dll",
				NameTr = @"SpyGlass Crystal Optics & Tactical Zoom",
				NameEn = @"SpyGlass Crystal Optics & Tactical Zoom",
				Version = @"v1.7.1",
				Category = @"Taktik & Keşif",
				DescTr = @"Dürbünün çatlak cam dokusunu kristal berraklığında temiz lensle değiştirir ve fare tekerleğiyle optik yakınlaştırma sunar.",
				DescEn = @"Replaces cracked spyglass glass texture with crystal optics, providing adjustable smooth zoom magnification.",
				NexusUrl = @"https://www.nexusmods.com/thislandismyland/mods/46",
				IsUpdated = true,
				UpdateTag = @"★ GÜNCELLENDİ (v1.7.1)"
			},
			new VaynaModsWindow.ModEntry
			{
				Id = @"com.vaynareka.vaynachat",
				DllName = @"VaynaChat.exe",
				NameTr = @"Vayna Reka Kabile Messenger & Canlı Çeviri",
				NameEn = @"Vayna Reka Tribal Messenger & Auto-Translator",
				Version = @"v1.0.0",
				Category = @"Ağ & Canlı Sohbet",
				DescTr = @"Oyun içi/dışı canlı kabile sohbeti, anlık çift yönlü İngilizce <-> Türkçe çeviri ve posta kutusu yönetimi masaüstü aracı.",
				DescEn = @"Standalone tribal chat client with real-time English <-> Turkish auto-translation and mailbox notifications.",
				NexusUrl = @"https://vayna.com.tr/chat/",
				IsUpdated = true,
				UpdateTag = @"★ YENİ (v1.0.0)"
			}
		};

		// Token: 0x02000045 RID: 69
		public enum ModState
		{
			// Token: 0x04000092 RID: 146
			Active,
			// Token: 0x04000093 RID: 147
			Disabled,
			// Token: 0x04000094 RID: 148
			NotInstalled
		}

		// Token: 0x02000046 RID: 70
		public class ModEntry
		{
			// Token: 0x04000095 RID: 149
			public string Id;

			// Token: 0x04000096 RID: 150
			public string DllName;

			// Token: 0x04000097 RID: 151
			public string NameTr;

			// Token: 0x04000098 RID: 152
			public string NameEn;

			// Token: 0x04000099 RID: 153
			public string Version;

			// Token: 0x0400009A RID: 154
			public string Category;

			// Token: 0x0400009B RID: 155
			public string DescTr;

			// Token: 0x0400009C RID: 156
			public string DescEn;

			// Token: 0x0400009D RID: 157
			public string NexusUrl;

			// Token: 0x0400009E RID: 158
			public bool IsUpdated;

			// Token: 0x0400009F RID: 159
			public string UpdateTag;
		}
	}
}
