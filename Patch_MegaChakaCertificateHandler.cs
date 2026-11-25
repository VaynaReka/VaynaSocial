using System;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000008 RID: 8
	[HarmonyPatch(typeof(MegaChakaCertificateHandler), "ValidateCertificate")]
	internal static class Patch_MegaChakaCertificateHandler
	{
		// Token: 0x06000040 RID: 64 RVA: 0x0000403D File Offset: 0x0000223D
		[HarmonyPrefix]
		private static bool Prefix(ref bool __result)
		{
			__result = true;
			return false;
		}
	}
}
