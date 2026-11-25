using System;
using System.Collections.Generic;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(SimpleChat), "SendChatMessage")]
	internal static class Patch_SimpleChat_SendChatMessage
	{
		[HarmonyPrefix]
		private static bool Prefix(SimpleChat __instance, string message, string channel)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				return false;
			}

			string trimmed = message.Trim();
			if (trimmed.Equals("/clear", StringComparison.OrdinalIgnoreCase) ||
			    trimmed.Equals("/temizle", StringComparison.OrdinalIgnoreCase) ||
			    trimmed.Equals("/cls", StringComparison.OrdinalIgnoreCase))
			{
				ClearAllChat(__instance);
				return false;
			}

			string text = string.IsNullOrEmpty(channel) ? "Global" : channel;
			string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();
			MegaChakaConnection.PlayerId = playerDisplayName;
			long nowTicks = DateTime.UtcNow.Ticks;
			SimpleChat.ChatMessage msgObj = new SimpleChat.ChatMessage
			{
				From = playerDisplayName,
				To = text,
				Message = message,
				Time = nowTicks,
				Type = 0
			};
			try
			{
				__instance.MessageReceived(msgObj, false);
			}
			catch
			{
			}
			ChatEngine.RecordRecentlySentLocal(message);
			ChatEngine.AddRecentMessage(msgObj);

			ChatEngine instance = ChatEngine.Instance;
			if (instance != null)
			{
				instance.SendChatMessage(message, text, nowTicks);
			}
			return false;
		}

		public static void ClearAllChat(SimpleChat chat)
		{
			try
			{
				ChatEngine.RecentMessages.Clear();

				if (chat != null && chat.ActiveChannels != null)
				{
					foreach (var kvp in chat.ActiveChannels)
					{
						if (kvp.Value != null && kvp.Value.ChannelMessages != null)
						{
							kvp.Value.ChannelMessages.Clear();
						}
					}
					var traverse = Traverse.Create(chat);
					var activeChan = traverse.Field<SimpleChat_Channel>("activeChannel").Value;
					if (activeChan != null)
					{
						traverse.Method("SelectChannel", new object[] { activeChan, true }).GetValue();
					}
				}

				if (InformationalScreen.Instance != null)
				{
					InformationalScreen.Instance.AddMessage("<color=#7CFC00>[Sohbet] Sohbet gecmisi temizlendi.</color>", InformationalScreen_InGame.EMessageSize.Large, 3f, 0.5f, false);
				}
			}
			catch (Exception ex)
			{
				VaynaSocialPlugin.Log?.LogWarning("ClearAllChat hatasi: " + ex);
			}
		}
	}
}