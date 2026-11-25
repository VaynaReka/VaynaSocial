using System;

using System.Collections.Generic;

using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;



namespace VaynaSocial

{

	// Token: 0x02000030 RID: 48

	[HarmonyPatch(typeof(SimpleChat), "Update")]

	internal static class Patch_SimpleChat_Update

	{

		// Token: 0x0600007B RID: 123 RVA: 0x00006CA4 File Offset: 0x00004EA4

		[HarmonyPrefix]

		private static void Prefix(SimpleChat __instance)

		{

			if (__instance == null)

			{

				return;

			}

			if (__instance.FullChat != null && __instance.FullChat.activeSelf && __instance.Canvas != null && !__instance.Canvas.enabled)

			{

				__instance.Canvas.enabled = true;

			}

			if (Time.time - Patch_SimpleChat_Update.lastCheckTime < 1f)

			{

				return;

			}

			Patch_SimpleChat_Update.lastCheckTime = Time.time;

			Traverse traverse = Traverse.Create(__instance);

			if (traverse.Field<SimpleChat_Channel>("activeChannel").Value == null || !traverse.Field<bool>("isInited").Value)

			{

				Patch_SimpleChat_Update.EnsureChatInitialized(__instance);

			}

		}



		// Token: 0x0600007C RID: 124 RVA: 0x00006D54 File Offset: 0x00004F54

		public static void EnsureChatInitialized(SimpleChat chat)

		{

			if (chat == null)

			{

				return;

			}

			try

			{

				MegaChakaConnection.OfflineMode = false;

				MegaChakaConnection.Connected = true;

				Traverse traverse = Traverse.Create(chat);

				traverse.Field<bool>("isConnected").Value = true;

				traverse.Field<bool>("isInited").Value = true;

				chat.isChatEnabled = true;

				chat.isSocialEnabled = true;

				Traverse<Dictionary<string, SimpleChat.ChatChannel>> traverse2 = traverse.Field<Dictionary<string, SimpleChat.ChatChannel>>("defaultChannels");

				if (traverse2.Value == null)

				{

					traverse2.Value = new Dictionary<string, SimpleChat.ChatChannel>();

				}

				string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();

				traverse.Field<string>("currentPlayerId").Value = playerDisplayName;

				MegaChakaConnection.PlayerId = playerDisplayName;

				if (chat.ConnectingLabel != null && chat.ConnectingLabel.activeSelf)

				{

					chat.ConnectingLabel.SetActive(false);

				}

				if (chat.ActiveChannels == null)

				{

					chat.ActiveChannels = new Dictionary<string, SimpleChat_Channel>();

				}

				SimpleChat_Channel simpleChat_Channel;

				if (chat.ActiveChannels.TryGetValue("Kabile", out simpleChat_Channel) && simpleChat_Channel != null)

				{

					try

					{

						traverse.Method("OnChannelClosed", new object[]

						{

							simpleChat_Channel

						}).GetValue();

					}

					catch

					{

					}

				}

				if (chat.ActiveChannels.Count == 0)

				{

					chat.AddChannel("Global", false, false);

				}

				Traverse<SimpleChat_Channel> traverse3 = traverse.Field<SimpleChat_Channel>("activeChannel");

				if (traverse3.Value == null)

				{

					if (chat.ActiveChannels.ContainsKey("Global"))

					{

						traverse3.Value = chat.ActiveChannels["Global"];

					}

					else

					{

						using (Dictionary<string, SimpleChat_Channel>.Enumerator enumerator = chat.ActiveChannels.GetEnumerator())

						{

							if (enumerator.MoveNext())

							{

								KeyValuePair<string, SimpleChat_Channel> keyValuePair = enumerator.Current;

								traverse3.Value = keyValuePair.Value;

							}

						}

					}

					SimpleChat_Channel value = traverse3.Value;

					if (value != null && value.ChannelToggle != null)

					{

						value.ChannelToggle.isOn = true;

					}

				}

				if (chat.CurrentInput != null && !chat.CurrentInput.gameObject.activeSelf)

				{

					chat.CurrentInput.gameObject.SetActive(true);

				}



				// Sahte / bot baslangic mesajlarini kanallardan temizle

				if (chat.ActiveChannels != null)

				{

					foreach (var kvp in chat.ActiveChannels)

					{

						if (kvp.Value != null && kvp.Value.ChannelMessages != null)

						{

							kvp.Value.ChannelMessages.RemoveAll(m => m == null || IsDummyChatMessage(m.Message));

						}

					}

				}

				ChatEngine.RecentMessages.RemoveAll(m => m == null || IsDummyChatMessage(m.Message));



			}

			catch (Exception ex)

			{

				ManualLogSource log = VaynaSocialPlugin.Log;

				if (log != null)

				{

					log.LogWarning("EnsureChatInitialized hatası: " + ex.Message);

				}

			}

		}



		public static void SyncRecentMessagesToChat(SimpleChat chat)

		{

			if (chat == null || ChatEngine.RecentMessages == null || ChatEngine.RecentMessages.Count == 0)

			{

				return;

			}

			try

			{

				SimpleChat_Channel globalChan = null;

				if (chat.ActiveChannels != null)

				{

					chat.ActiveChannels.TryGetValue("Global", out globalChan);

				}



				SimpleChat.ChatMessage[] array = ChatEngine.RecentMessages.ToArray();

				for (int i = 0; i < array.Length; i++)

				{

					SimpleChat.ChatMessage msg = array[i];

					if (msg == null || string.IsNullOrWhiteSpace(msg.Message) || IsDummyChatMessage(msg.Message))

					{

						continue;

					}



					bool alreadyPresent = false;

					if (globalChan != null && globalChan.ChannelMessages != null)

					{

						for (int j = 0; j < globalChan.ChannelMessages.Count; j++)

						{

							SimpleChat.ChatMessage existing = globalChan.ChannelMessages[j];

							if (existing == null) continue;



							bool sameSender = string.Equals(existing.From?.Trim(), msg.From?.Trim(), StringComparison.OrdinalIgnoreCase);

							bool sameText = string.Equals(existing.Message?.Trim(), msg.Message?.Trim(), StringComparison.OrdinalIgnoreCase);



							if (sameSender && sameText)

							{



								{

									alreadyPresent = true;

									break;

								}

							}

						}

					}



					if (!alreadyPresent)

					{

						try

						{

							chat.MessageReceived(msg, false);

						}

						catch

						{

						}

					}

				}

			}

			catch (Exception ex)

			{

				ManualLogSource log = VaynaSocialPlugin.Log;

				if (log != null)

				{

					log.LogWarning("SyncRecentMessagesToChat hatasi: " + ex.Message);

				}

			}

		}



		private static bool IsDummyChatMessage(string msg)

		{

			if (string.IsNullOrEmpty(msg)) return false;

			return msg.IndexOf("Sovereign Tribal Network", StringComparison.OrdinalIgnoreCase) >= 0 ||

			       msg.IndexOf("happy to see the sovereign", StringComparison.OrdinalIgnoreCase) >= 0 ||

			       msg.IndexOf("Everything is running smoothly", StringComparison.OrdinalIgnoreCase) >= 0 ||

			       msg.IndexOf("Greetings from tortuga", StringComparison.OrdinalIgnoreCase) >= 0;

		}



		// Token: 0x04000032 RID: 50

		private static float lastCheckTime;

	}

}

