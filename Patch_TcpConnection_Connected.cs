using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x0200000A RID: 10
	[HarmonyPatch(typeof(TcpConnection), "Connected", MethodType.Getter)]
	internal static class Patch_TcpConnection_Connected
	{
		// Token: 0x06000042 RID: 66 RVA: 0x00004090 File Offset: 0x00002290
		[HarmonyPrefix]
		private static bool Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}
}
