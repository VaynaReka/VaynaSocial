using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VaynaSocial
{
	// Token: 0x02000010 RID: 16
	[HarmonyPatch(typeof(SocialWindow), "OnEnable")]
	internal static class Patch_SocialWindow_OnEnable
	{
		// Token: 0x0600004F RID: 79 RVA: 0x000048CC File Offset: 0x00002ACC
		[HarmonyPostfix]
		private static void Postfix(SocialWindow __instance)
		{
			MegaChakaConnection.Connected = true;
			MegaChakaConnection.OfflineMode = false;
			string playerDisplayName = VaynaSocialPlugin.GetPlayerDisplayName();
			MegaChakaConnection.PlayerId = playerDisplayName;
			if (WorldState.Instance != null)
			{
				WorldState.Instance.WorldSocialBanned = false;
			}
			TradingEngine.EnsureBarterAuctionItem();
			VaynaSocialPlugin.EnsureProfileLoaded();

			if (__instance != null)
			{
				if (__instance.NotConnectedImage != null)
				{
					__instance.NotConnectedImage.gameObject.SetActive(false);
				}
				if (__instance.SocialLimited != null)
				{
					__instance.SocialLimited.SetActive(false);
				}
				if (__instance.OnlineStatusText != null)
				{
					__instance.OnlineStatusText.text = "<color=#55FF55>CONNECTED (Tribal Network)</color>";
				}
				if (__instance.OnlineTagText != null)
				{
					__instance.OnlineTagText.text = playerDisplayName;
				}
				if (SocialManager.Profile != null && SocialManager.Barter != null && SocialManager.Barter.AuctionItem != null && __instance.TokensAmountText != null)
				{
					__instance.TokensAmountText.text = SocialManager.Profile.GetItemCount(SocialManager.Barter.AuctionItem).ToString();
				}
				try
				{
					if (__instance.TabButtonsPanel != null)
					{
						Transform transform = __instance.TabButtonsPanel.transform;
						if (transform.Find("VaynaModsTabButton") == null)
						{
							Button componentInChildren = transform.GetComponentInChildren<Button>();
							if (componentInChildren != null)
							{
								GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(componentInChildren.gameObject, transform, false);
								gameObject.name = "VaynaModsTabButton";
								gameObject.transform.SetAsLastSibling();
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
								Text componentInChildren2 = gameObject.GetComponentInChildren<Text>();
								if (componentInChildren2 != null)
								{
									componentInChildren2.text = "MODLAR / MODS";
									componentInChildren2.color = new Color(1f, 0.86f, 0.42f);
								}
								if (transform.Find("VaynaDiscordTabButton") == null)
								{
									GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(componentInChildren.gameObject, transform, false);
									gameObject2.name = "VaynaDiscordTabButton";
									gameObject2.transform.SetAsLastSibling();
									componentsInChildren = gameObject2.GetComponentsInChildren<LocalizeText>(true);
									for (int i = 0; i < componentsInChildren.Length; i++)
									{
										UnityEngine.Object.Destroy(componentsInChildren[i]);
									}
									Button component2 = gameObject2.GetComponent<Button>();
									if (component2 != null)
									{
										component2.onClick = new Button.ButtonClickedEvent();
										component2.onClick.AddListener(delegate()
										{
											VaynaModsWindow.OpenDiscordInvite();
										});
									}
									Text componentInChildren3 = gameObject2.GetComponentInChildren<Text>();
									if (componentInChildren3 != null)
									{
										componentInChildren3.text = "DISCORD ↗";
										componentInChildren3.color = new Color(0.45f, 0.65f, 1f);
									}
								}
							}
						}
					}
				}
				catch
				{
				}
			}
		}
	}
}
