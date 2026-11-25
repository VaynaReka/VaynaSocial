using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000016 RID: 22
	[HarmonyPatch(typeof(PlayerActions), "checkSaveAbility")]
	internal static class Patch_PlayerActions_checkSaveAbility
	{
		// Token: 0x06000056 RID: 86 RVA: 0x00004E64 File Offset: 0x00003064
		[HarmonyPrefix]
		private static bool Prefix(ref string __result)
		{
			if (Mailbox_Compose.IsActive)
			{
				__result = null;
				return false;
			}
			StackTrace stackTrace = new StackTrace(2, false);
			for (int i = 0; i < Math.Min(10, stackTrace.FrameCount); i++)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				MethodBase methodBase = (frame != null) ? frame.GetMethod() : null;
				string text;
				if (methodBase == null)
				{
					text = null;
				}
				else
				{
					Type declaringType = methodBase.DeclaringType;
					text = ((declaringType != null) ? declaringType.FullName : null);
				}
				string text2 = text ?? "";
				if (text2.IndexOf("Mailbox", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("Attach", StringComparison.OrdinalIgnoreCase) >= 0 || text2.IndexOf("Social", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					__result = null;
					return false;
				}
			}
			return true;
		}
	}
}
