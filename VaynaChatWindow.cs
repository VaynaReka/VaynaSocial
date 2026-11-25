using System;
using System.Collections.Generic;
using UnityEngine;

namespace VaynaSocial
{
	// Token: 0x02000033 RID: 51
	public class VaynaChatWindow : MonoBehaviour
	{
		// Token: 0x0600008B RID: 139 RVA: 0x00008C83 File Offset: 0x00006E83
		private void Awake()
		{
			this.bgTex = new Texture2D(1, 1);
			this.bgTex.SetPixel(0, 0, new Color(0.08f, 0.07f, 0.06f, 0.94f));
			this.bgTex.Apply();
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00008CC4 File Offset: 0x00006EC4
		private void InitStyles()
		{
			if (this.windowStyle != null)
			{
				return;
			}
			this.windowStyle = new GUIStyle(GUI.skin.box);
			this.windowStyle.normal.background = this.bgTex;
			this.windowStyle.padding = new RectOffset(16, 16, 14, 14);
			this.titleStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 15,
				fontStyle = FontStyle.Bold,
				normal = 
				{
					textColor = new Color(1f, 0.85f, 0.42f)
				}
			};
			this.channelActiveStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 12,
				fontStyle = FontStyle.Bold,
				normal = 
				{
					textColor = Color.yellow
				}
			};
			this.channelInactiveStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 12,
				fontStyle = FontStyle.Normal,
				normal = 
				{
					textColor = Color.white
				}
			};
			this.msgStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 12,
				wordWrap = true,
				richText = true,
				normal = 
				{
					textColor = new Color(0.92f, 0.92f, 0.92f)
				}
			};
			this.inputStyle = new GUIStyle(GUI.skin.textField)
			{
				fontSize = 13,
				normal = 
				{
					textColor = Color.white
				}
			};
			this.sendButtonStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 13,
				fontStyle = FontStyle.Bold,
				normal = 
				{
					textColor = new Color(1f, 0.88f, 0.45f)
				}
			};
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00008E82 File Offset: 0x00007082
		public static void Open()
		{
			VaynaChatWindow.IsOpen = true;
			VaynaChatWindow.focusInputNext = true;
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			HUD.MouseNeeded = true;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00008EA2 File Offset: 0x000070A2
		public static void Close()
		{
			VaynaChatWindow.IsOpen = false;
			HUD.MouseNeeded = false;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00008EB0 File Offset: 0x000070B0
		private void OnGUI()
		{
			if (!VaynaChatWindow.IsOpen)
			{
				return;
			}
			this.InitStyles();
			float num = Mathf.Min(560f, (float)Screen.width - 40f);
			float num2 = Mathf.Min(440f, (float)Screen.height - 40f);
			float num3 = 24f;
			float num4 = (float)Screen.height - num2 - 30f;
			GUI.Box(new Rect(num3, num4, num, num2), GUIContent.none, this.windowStyle);
			GUILayout.BeginArea(new Rect(num3 + 12f, num4 + 10f, num - 24f, num2 - 20f));
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label("\ud83d\udcac CANLI OYUN SOHBETİ", this.titleStyle, Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Global", this.channelActiveStyle, new GUILayoutOption[]
			{
				GUILayout.Width(75f),
				GUILayout.Height(26f)
			}))
			{
				VaynaChatWindow.currentChannel = "Global";
			}
			GUILayout.Space(10f);
			if (GUILayout.Button("✕", this.sendButtonStyle, new GUILayoutOption[]
			{
				GUILayout.Width(28f),
				GUILayout.Height(26f)
			}))
			{
				VaynaChatWindow.Close();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(6f);
			float height = num2 - 90f;
			VaynaChatWindow.scrollPos = GUILayout.BeginScrollView(VaynaChatWindow.scrollPos, new GUILayoutOption[]
			{
				GUILayout.Height(height)
			});
			List<SimpleChat.ChatMessage> recentMessages = ChatEngine.RecentMessages;
			string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();
			if (recentMessages == null || recentMessages.Count == 0)
			{
				GUILayout.Label("<color=#888888><i>Henüz bir sohbet mesajı yok. İlk mesajı yazmak için aşağıya yazıp Gönder'e basın...</i></color>", this.msgStyle, Array.Empty<GUILayoutOption>());
			}
			else
			{
				for (int i = 0; i < recentMessages.Count; i++)
				{
					SimpleChat.ChatMessage chatMessage = recentMessages[i];
					if (chatMessage != null)
					{
						string text = string.IsNullOrEmpty(chatMessage.To) ? "Global" : chatMessage.To;
						if (!(VaynaChatWindow.currentChannel != "All") || !(text != VaynaChatWindow.currentChannel) || !(text != "All"))
						{
							string text2 = "";
							if (chatMessage.Time > 0L)
							{
								try
								{
									text2 = new DateTime(chatMessage.Time, DateTimeKind.Utc).ToLocalTime().ToString("HH:mm");
								}
								catch
								{
								}
							}
							string text3 = chatMessage.From;
							string text4 = "#88CCFF";
							if (string.Equals(text3, playerDisplayName, StringComparison.OrdinalIgnoreCase))
							{
								text4 = "#FFDD55";
								text3 = playerDisplayName + " (Siz)";
							}
							else if (text3 == "[System]" || text3 == "System")
							{
								text4 = "#55FF55";
							}
							string text5 = "<color=#44AAFF>[Global]</color>";
							if (!string.IsNullOrEmpty(text) && !text.Equals("Global", StringComparison.OrdinalIgnoreCase) && !text.Equals("All", StringComparison.OrdinalIgnoreCase))
							{
								text5 = "<color=#FF9944>[Özel: " + text + "]</color>";
							}
							GUILayout.Label(string.Concat(new string[]
							{
								"<color=#888888>[",
								text2,
								"]</color> ",
								text5,
								" <color=",
								text4,
								"><b>",
								text3,
								":</b></color> ",
								chatMessage.Message
							}), this.msgStyle, Array.Empty<GUILayoutOption>());
						}
					}
				}
			}
			GUILayout.EndScrollView();
			GUILayout.Space(6f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUI.SetNextControlName("ChatInputField");
			VaynaChatWindow.inputMessage = GUILayout.TextField(VaynaChatWindow.inputMessage, this.inputStyle, new GUILayoutOption[]
			{
				GUILayout.Height(30f)
			});
			if (VaynaChatWindow.focusInputNext)
			{
				GUI.FocusControl("ChatInputField");
				VaynaChatWindow.focusInputNext = false;
			}
			bool flag = GUILayout.Button("Gönder", this.sendButtonStyle, new GUILayoutOption[]
			{
				GUILayout.Width(80f),
				GUILayout.Height(30f)
			});
			bool flag2 = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
			if ((flag || (flag2 && GUI.GetNameOfFocusedControl() == "ChatInputField")) && !string.IsNullOrWhiteSpace(VaynaChatWindow.inputMessage))
			{
				string text6 = VaynaChatWindow.inputMessage.Trim();
				VaynaChatWindow.inputMessage = "";
				MegaChakaConnection.PlayerId = playerDisplayName;
				ChatEngine instance = ChatEngine.Instance;
				if (instance != null)
				{
					instance.SendChatMessage(text6, VaynaChatWindow.currentChannel);
				}
				if (SimpleChat.Instance != null)
				{
					try
					{
						SimpleChat.ChatMessage message = new SimpleChat.ChatMessage
						{
							From = playerDisplayName,
							To = VaynaChatWindow.currentChannel,
							Message = text6,
							Time = DateTime.UtcNow.Ticks,
							Type = 0
						};
						SimpleChat.Instance.MessageReceived(message, false);
					}
					catch
					{
					}
				}
				VaynaChatWindow.scrollPos.y = float.MaxValue;
				VaynaChatWindow.focusInputNext = true;
				if (flag2)
				{
					Event.current.Use();
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
			{
				VaynaChatWindow.Close();
				Event.current.Use();
			}
		}

		// Token: 0x04000045 RID: 69
		public static bool IsOpen = false;

		// Token: 0x04000046 RID: 70
		private static Vector2 scrollPos = Vector2.zero;

		// Token: 0x04000047 RID: 71
		private static string currentChannel = "Global";

		// Token: 0x04000048 RID: 72
		private static string inputMessage = "";

		// Token: 0x04000049 RID: 73
		private static bool focusInputNext = false;

		// Token: 0x0400004A RID: 74
		private GUIStyle windowStyle;

		// Token: 0x0400004B RID: 75
		private GUIStyle titleStyle;

		// Token: 0x0400004C RID: 76
		private GUIStyle channelActiveStyle;

		// Token: 0x0400004D RID: 77
		private GUIStyle channelInactiveStyle;

		// Token: 0x0400004E RID: 78
		private GUIStyle msgStyle;

		// Token: 0x0400004F RID: 79
		private GUIStyle inputStyle;

		// Token: 0x04000050 RID: 80
		private GUIStyle sendButtonStyle;

		// Token: 0x04000051 RID: 81
		private Texture2D bgTex;
	}
}
