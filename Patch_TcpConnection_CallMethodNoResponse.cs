using System;
using HarmonyLib;
using UnityEngine.Events;

namespace VaynaSocial
{
	// Token: 0x02000014 RID: 20
	[HarmonyPatch(typeof(TcpConnection), "CallMethodNoResponse", new Type[]
	{
		typeof(MegaChakaConnection.EServiceType),
		typeof(string),
		typeof(string)
	})]
	internal static class Patch_TcpConnection_CallMethodNoResponse
	{
		// Token: 0x06000053 RID: 83 RVA: 0x00004D40 File Offset: 0x00002F40
		[HarmonyPrefix]
		private static bool Prefix(MegaChakaConnection.EServiceType serviceType, string methodName, string data)
		{
			if (serviceType == MegaChakaConnection.EServiceType.MailChatService)
			{
				if (MailEngine.IsMailMethod(methodName))
				{
					MailEngine instance = MailEngine.Instance;
					if (instance != null)
					{
						instance.Enqueue(methodName, data, null);
					}
					return false;
				}
				if (methodName == "PostChatMessage" && ChatEngine.Instance != null)
				{
					ChatEngine.Instance.HandleOutgoingMessage(data);
					return false;
				}
			}
			// Digerleri: HTTP ile sunucuya ilet (cevap beklenmiyor).
			MegaChakaConnection.CallMethod(serviceType, methodName, data, (UnityAction<string>)null, (UnityAction<byte[]>)null);
			return false;
		}
	}
}
