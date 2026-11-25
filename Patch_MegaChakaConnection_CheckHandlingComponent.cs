using System;
using System.Reflection;
using HarmonyLib;

namespace VaynaSocial
{
	// Token: 0x0200000D RID: 13
	[HarmonyPatch(typeof(MegaChakaConnection), "CheckHandlingComponent")]
	internal static class Patch_MegaChakaConnection_CheckHandlingComponent
	{
		// Token: 0x06000047 RID: 71 RVA: 0x00004120 File Offset: 0x00002320
		[HarmonyPostfix]
		[HarmonyPriority(800)]
		private static void Postfix()
		{
			try
			{
				FieldInfo fieldInfo = Patch_MegaChakaConnection_CheckHandlingComponent.handlingCompField;
				MegaChakaConnection megaChakaConnection = ((fieldInfo != null) ? fieldInfo.GetValue(null) : null) as MegaChakaConnection;
				if (!(megaChakaConnection == null))
				{
					string text = ModConfig.SocialServerUrl.Value.Trim();
					if (!text.EndsWith("/"))
					{
						text += "/";
					}
					if (!(Patch_MegaChakaConnection_CheckHandlingComponent.configuredUrl == text))
					{
						Uri uri = new Uri(text);
						FieldInfo fieldInfo2 = Patch_MegaChakaConnection_CheckHandlingComponent.addressField;
						if (fieldInfo2 != null)
						{
							fieldInfo2.SetValue(megaChakaConnection, text);
						}
						FieldInfo fieldInfo3 = Patch_MegaChakaConnection_CheckHandlingComponent.tcpAddressField;
						if (fieldInfo3 != null)
						{
							fieldInfo3.SetValue(megaChakaConnection, uri.Host);
						}
						if (Patch_MegaChakaConnection_CheckHandlingComponent.addressEndPointsField != null)
						{
							string[] array = new string[9];
							for (int i = 0; i < 9; i++)
							{
								array[i] = text;
							}
							Patch_MegaChakaConnection_CheckHandlingComponent.addressEndPointsField.SetValue(megaChakaConnection, array);
						}
						Patch_MegaChakaConnection_CheckHandlingComponent.configuredUrl = text;
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x0400002A RID: 42
		private static readonly FieldInfo handlingCompField = AccessTools.Field(typeof(MegaChakaConnection), "handlingComponent");

		// Token: 0x0400002B RID: 43
		private static readonly FieldInfo addressField = AccessTools.Field(typeof(MegaChakaConnection), "Address");

		// Token: 0x0400002C RID: 44
		private static readonly FieldInfo tcpAddressField = AccessTools.Field(typeof(MegaChakaConnection), "TcpAddress");

		// Token: 0x0400002D RID: 45
		private static readonly FieldInfo addressEndPointsField = AccessTools.Field(typeof(MegaChakaConnection), "AddressEndPoints");

		// Token: 0x0400002E RID: 46
		private static string configuredUrl = null;
	}
}
