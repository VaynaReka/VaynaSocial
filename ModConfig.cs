// Mod yapimcisi: Vayna Reka
// Bagimlilik: Bu mod VaynaCore.dll'ye baglidir (BepInEx/plugins icinde bulunmali).

using System;
using BepInEx.Configuration;

namespace VaynaSocial
{
	public static class ModConfig
	{
		// Varsayilan sunucu adresi (acik metin).
		private const string DefaultServerUrl = "https://raspy-fog-fb71.trvayna.workers.dev/";

		public static void Init(ConfigFile config)
		{
			string defaultServer = DefaultServerUrl;

			ModConfig.CustomChatDisplayName = config.Bind<string>("General", "CustomChatDisplayName", "", "Chat ve posta sisteminde gorunecek ozel oyuncu adi (bos birakilirsa Steam adi kullanilir).");
			ModConfig.SocialServerUrl = config.Bind<string>("Server", "SocialServerUrl", defaultServer, "Vayna Sosyal Sunucusu (Canli Sohbet & Kabile Agi).");
			ModConfig.MailServerUrl = config.Bind<string>("Server", "MailServerUrl", defaultServer, "Ozel Kabile Posta API Adresi.");
			ModConfig.DiscordInviteUrl = config.Bind<string>("Community", "DiscordInviteUrl", "https://discord.gg/HqJRZQZN4t", "Kabile Discord toplulugu davet linki.");
			ModConfig.EnableSocial = config.Bind<bool>("Features", "EnableSocial", true, "Sosyal ozellikler aktif edilsin mi?");
			ModConfig.EnableMail = config.Bind<bool>("Features", "EnableMail", true, "Posta sistemi aktif edilsin mi?");
			ModConfig.EnableInstantTravelAndRest = config.Bind<bool>("Features", "EnableInstantTravelAndRest", true, "Hizli seyahat ve kamp atesi dinlenmesindeki bekleme ekranini aninda gec (Varsayilan: true).");
			ModConfig.InstantRestDuration = config.Bind<float>("Features", "InstantRestDuration", 0.05f, "EnableInstantTravelAndRest aktifken bekleme suresi (saniye).");
			ModConfig.ChatPollInterval = config.Bind<float>("Features", "ChatPollInterval", 3f, "Sohbet acikken yeni mesajlari kontrol etme araligi (saniye).");
			ModConfig.DebugLog = config.Bind<bool>("Debug", "DebugLog", false, "Gelistirici loglarini konsola yazdir.");

			// Eski surumlerde varsayilan ad "Vayna Reka" idi; bir kereye mahsus sifirla (bos = Steam adi kullanilir).
			ConfigEntry<int> cfgVer = config.Bind<int>("Internal", "ConfigVersion", 0, "Dahili surum numarasi (degistirmeyin).");
			if (cfgVer.Value < 2)
			{
				if (string.Equals(ModConfig.CustomChatDisplayName.Value, "Vayna Reka", StringComparison.OrdinalIgnoreCase))
				{
					ModConfig.CustomChatDisplayName.Value = string.Empty;
				}
				cfgVer.Value = 2;
				config.Save();
			}

			// Varsayilan sunucu adresi bos ise ata
			if (string.IsNullOrEmpty(ModConfig.SocialServerUrl.Value))
			{
				ModConfig.SocialServerUrl.Value = defaultServer;
				config.Save();
			}
		}

		public static ConfigEntry<string> SocialServerUrl;
		public static ConfigEntry<string> MailServerUrl;
		public static ConfigEntry<string> CustomChatDisplayName;
		public static ConfigEntry<bool> EnableSocial;
		public static ConfigEntry<bool> EnableMail;
		public static ConfigEntry<bool> EnableInstantTravelAndRest;
		public static ConfigEntry<float> InstantRestDuration;
		public static ConfigEntry<float> ChatPollInterval;
		public static ConfigEntry<bool> DebugLog;
		public static ConfigEntry<string> DiscordInviteUrl;
	}
}
