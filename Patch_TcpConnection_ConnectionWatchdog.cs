using System;
using System.Collections;
using HarmonyLib;

namespace VaynaSocial
{
	[HarmonyPatch(typeof(TcpConnection), "ConnectionWatchdog")]
	internal static class Patch_TcpConnection_ConnectionWatchdog
	{
		[HarmonyPrefix]
		private static bool Prefix(ref IEnumerator __result)
		{
			__result = Patch_TcpConnection_ConnectionWatchdog.EmptyCoroutine();
			return false;
		}

		private static IEnumerator EmptyCoroutine()
		{
			yield break;
		}
	}
}
