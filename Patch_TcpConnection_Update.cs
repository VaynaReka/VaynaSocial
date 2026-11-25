using System;
using System.Reflection;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x0200000C RID: 12
	[HarmonyPatch(typeof(TcpConnection), "Update")]
	internal static class Patch_TcpConnection_Update
	{
		// Token: 0x06000045 RID: 69 RVA: 0x000040A8 File Offset: 0x000022A8
		[HarmonyPostfix]
		private static void Postfix()
		{
			if (MegaChakaConnection.Connected && TcpConnection.Instance != null && Patch_TcpConnection_Update.lastReceivedField != null)
			{
				try
				{
					Patch_TcpConnection_Update.lastReceivedField.SetValue(TcpConnection.Instance, DateTime.UtcNow);
				}
				catch
				{
				}
			}
		}

		// Token: 0x04000029 RID: 41
		private static readonly FieldInfo lastReceivedField = AccessTools.Field(typeof(TcpConnection), "LastReceivedMessageTime");
	}
}
