using System;
using HarmonyLib;
using UnityEngine.Events;

namespace VaynaSocial
{
	// Token: 0x02000013 RID: 19
	[HarmonyPatch(typeof(TcpConnection), "CallMethod", new Type[]
	{
		typeof(MegaChakaConnection.EServiceType),
		typeof(string),
		typeof(string),
		typeof(UnityAction<string>),
		typeof(bool)
	})]
	internal static class Patch_TcpConnection_CallMethod
	{
		// Token: 0x06000052 RID: 82 RVA: 0x00004C7C File Offset: 0x00002E7C
		[HarmonyPrefix]
		private static bool Prefix(MegaChakaConnection.EServiceType serviceType, string methodName, string data, UnityAction<string> responseHandler, bool isLogin)
		{
			if (serviceType == MegaChakaConnection.EServiceType.MailChatService)
			{
				if (MailEngine.IsMailMethod(methodName))
				{
					MailEngine instance = MailEngine.Instance;
					if (instance != null)
					{
						instance.Enqueue(methodName, data, responseHandler);
					}
					return false;
				}
				if (methodName == "GetAvailableChannels")
				{
					string arg = "{\"Global\":{\"SocialName\":\"Global\",\"ChannelName\":\"Global\",\"LastMessages\":[]}}";
					if (responseHandler != null)
					{
						responseHandler(arg);
					}
					return false;
				}
				if (methodName == "VoteForMute")
				{
					if (responseHandler != null)
					{
						responseHandler("OK");
					}
					return false;
				}
				if (methodName == "GetChatParticipants")
				{
					string arg2 = "[\"Vayna Reka\",\"ThunderBear\",\"ShadowRaven\",\"SwiftArrow\",\"IronWolf\",\"SilentRiver\",\"StormRider\",\"GhostFox\",\"EagleFeather\",\"SilverHawk\",\"RedWillow\",\"BraveBison\"]";
					if (responseHandler != null)
					{
						responseHandler(arg2);
					}
					return false;
				}
				if (methodName == "PostChatMessage" && ChatEngine.Instance != null)
				{
					ChatEngine.Instance.HandleOutgoingMessage(data);
					if (responseHandler != null)
					{
						responseHandler("OK");
					}
					return false;
				}
			}
			// Posta/sohbet disindaki tum cagrilar (ticaret, profil, siralama...): sahte TCP baglantisina
			// gitmesin, HTTP ile VaynaSocial sunucusuna iletilsin (eski modun RouteTcpRequest mantigi).
			MegaChakaConnection.CallMethod(serviceType, methodName, data, responseHandler, (UnityAction<byte[]>)null);
			return false;
		}
	}
}
