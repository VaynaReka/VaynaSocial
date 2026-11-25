using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VaynaSocial
{
	// Token: 0x0200000F RID: 15
	[HarmonyPatch(typeof(MainMenuHandler), "Update")]
	internal static class Patch_MainMenuHandler_Update
	{
		// Token: 0x0600004E RID: 78 RVA: 0x0000463C File Offset: 0x0000283C
		[HarmonyPostfix]
		private static void Postfix(MainMenuHandler __instance)
		{
			if (__instance == null)
			{
				return;
			}
			try
			{
				if (__instance.MenuButtonsLayout != null)
				{
					Transform transform = __instance.MenuButtonsLayout.transform;
					Transform transform2 = transform.Find("VaynaDiscordButton");
					if (transform2 != null)
					{
						UnityEngine.Object.Destroy(transform2.gameObject);
					}
					if (__instance.ExitGameButton != null && __instance.ExitGameButton.transform.parent != null)
					{
						Transform parent = __instance.ExitGameButton.transform.parent;
						Transform transform3 = parent.Find("VaynaDiscordButton");
						if (transform3 != null)
						{
							UnityEngine.Object.Destroy(transform3.gameObject);
						}
						Transform transform4 = parent.Find("VaynaModsButton");
						if (transform4 != null && parent != transform)
						{
							UnityEngine.Object.Destroy(transform4.gameObject);
						}
					}
					if (transform.Find("VaynaModsButton") == null)
					{
						Button button;
						if ((button = __instance.NewGameButton) == null)
						{
							button = (__instance.ContinueGameButton ?? __instance.SwitchResistanceButton);
						}
						Button button2 = button;
						if (button2 != null)
						{
							GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(button2.gameObject, transform, false);
							gameObject.name = "VaynaModsButton";
							int siblingIndex = (__instance.ExitGameButton != null) ? __instance.ExitGameButton.transform.GetSiblingIndex() : transform.childCount;
							gameObject.transform.SetSiblingIndex(siblingIndex);
							LocalizeText[] componentsInChildren = gameObject.GetComponentsInChildren<LocalizeText>(true);
							for (int i = 0; i < componentsInChildren.Length; i++)
							{
								UnityEngine.Object.Destroy(componentsInChildren[i]);
							}
							Button component = gameObject.GetComponent<Button>();
							if (component != null)
							{
								component.onClick = new Button.ButtonClickedEvent();
								component.onClick.AddListener(delegate()
								{
									VaynaModsWindow.IsOpen = !VaynaModsWindow.IsOpen;
								});
							}
							Text componentInChildren = gameObject.GetComponentInChildren<Text>();
							if (componentInChildren != null)
							{
								componentInChildren.text = "MODLAR / MODS";
								componentInChildren.color = new Color(1f, 0.86f, 0.42f);
							}
						}
					}
				}
			}
			catch
			{
			}
			if (Time.unscaledTime < Patch_MainMenuHandler_Update.nextUpdate)
			{
				return;
			}
			Patch_MainMenuHandler_Update.nextUpdate = Time.unscaledTime + 0.5f;
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			VaynaSocialPlugin.EnsureChatReady();
			if (__instance.ConnectionStatusText != null)
			{
				string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();
				__instance.ConnectionStatusText.text = "<color=#55FF55>CONNECTED (Tribal Network)</color> " + playerDisplayName;
			}
		}

		// Token: 0x0400002F RID: 47
		private static float nextUpdate;
	}
}
