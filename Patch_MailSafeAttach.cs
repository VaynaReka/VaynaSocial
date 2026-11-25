using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x02000015 RID: 21
	[HarmonyPatch(typeof(GameplayExtensions), "SaveAllowed")]
	public static class Patch_MailSafeAttach
	{
		// Token: 0x06000054 RID: 84 RVA: 0x00004D96 File Offset: 0x00002F96
		[HarmonyPrefix]
		public static bool Prefix(ref bool __result, ref string reason)
		{
			if (Mailbox_Compose.IsActive)
			{
				__result = true;
				reason = string.Empty;
				return false;
			}
			return true;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00004DAC File Offset: 0x00002FAC
		[HarmonyPostfix]
		public static void Postfix(ref bool __result, ref string reason)
		{
			if (Mailbox_Compose.IsActive)
			{
				__result = true;
				reason = string.Empty;
				return;
			}
			if (!__result)
			{
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
						__result = true;
						reason = string.Empty;
						return;
					}
				}
			}
		}
	}
}
