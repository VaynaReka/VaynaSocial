using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000009 RID: 9
	[HarmonyPatch(typeof(TcpConnection), "Connect")]
	internal static class Patch_TcpConnection_Connect
	{
		// Token: 0x06000041 RID: 65 RVA: 0x00004044 File Offset: 0x00002244
		[HarmonyPrefix]
		private static bool Prefix()
		{
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			try
			{
				TcpConnection instance = TcpConnection.Instance;
				if (instance != null)
				{
					Action onConnect = instance.OnConnect;
					if (onConnect != null)
					{
						onConnect();
					}
				}
			}
			catch
			{
			}
			return false;
		}
	}
}
