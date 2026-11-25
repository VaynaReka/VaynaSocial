using System;
using HarmonyLib;

namespace VaynaSocial
{
    [HarmonyPatch(typeof(SimpleChat_Channel), "AddMessage")]
    internal static class Patch_SimpleChat_Channel_AddMessage
    {
        private static bool Prefix(SimpleChat_Channel __instance, SimpleChat.ChatMessage message)
        {
            if (message == null) return false;
            string msg = message.Message ?? "";

            // Duplicate guard: Prevent duplicate chat messages from the same sender in the channel
            if (__instance != null && __instance.ChannelMessages != null && __instance.ChannelMessages.Count > 0)
            {
                int start = Math.Max(0, __instance.ChannelMessages.Count - 6);
                for (int i = __instance.ChannelMessages.Count - 1; i >= start; i--)
                {
                    var prev = __instance.ChannelMessages[i];
                    if (prev != null &&
                        string.Equals(prev.From?.Trim(), message.From?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(prev.Message?.Trim(), message.Message?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
