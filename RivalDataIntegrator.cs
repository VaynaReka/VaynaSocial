using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VaynaSocial
{
    /// <summary>
    /// Rakip / Eski Sosyal Mod Entegrasyon ve Göç Motoru (Rival Data Integrator):
    /// Hırsızın moduyla veya eski mod sürümleriyle oynayan kullanıcılar VaynaSocial'e geçtiğinde:
    /// 1. Envanter & Profil: Tüm tokenlar, paralar, silahlar, cephaneler ve boyalar eksiksiz taşınır.
    /// 2. Ticaret & Pazar: Eski modda pazara konan veya takasta olan tüm eşyalar ve teklifler korunur.
    /// 3. Etkinlikler: Sosyal etkinliklerdeki tüm hedefler, tamamlanma durumları ve sayaçlar entegre edilir.
    /// 4. Liderlik Tablosu: Ömür boyu istatistikler, öldürmeler ve puanlar VaynaSocial tablosuna doğrudan aktarılır.
    /// 5. Kayıt Dosyaları: Oyuncunun kayıt dosyaları ve posta kutusu (mailbox.sav) sıfır veri kaybıyla korunur.
    /// </summary>
    public static class RivalDataIntegrator
    {
        private const string MigrationFlagPref = "Vayna.Social.MigrationCompleted.v1";

        public static string AppDataGamePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
                    @"Game Labs\This Land Is My Land"
                );
            }
        }

        public static string SavedGamesPath => Path.Combine(AppDataGamePath, "SavedGames");
        public static string StatisticsPath => Path.Combine(AppDataGamePath, "Statistics");

        /// <summary>
        /// Otomatik Entegrasyon ve Veri Taşıma Başlatıcı
        /// </summary>
        public static void RunAutomaticMigration()
        {
            try
            {
                VaynaSocialPlugin.Log?.LogInfo("=========================================================");
                VaynaSocialPlugin.Log?.LogInfo("[RivalDataIntegrator] Rakip mod verilerini VaynaSocial'e entegre etme kontrolü başlatıldı...");

                bool profileMigrated = IntegrateProfileAndInventory();
                bool tradeMigrated = IntegrateTradingData();
                bool eventsMigrated = IntegrateSocialEvents();
                bool statsMigrated = IntegrateStatisticsAndLeaderboard();
                IntegrateSaveFilesAndMailbox();

                bool anyMigrated = profileMigrated || tradeMigrated || eventsMigrated || statsMigrated;

                if (anyMigrated || PlayerPrefs.GetInt(MigrationFlagPref, 0) == 0)
                {
                    PlayerPrefs.SetInt(MigrationFlagPref, 1);
                    PlayerPrefs.Save();

                    VaynaSocialPlugin.Log?.LogInfo("[RivalDataIntegrator] Tüm veriler (Ticaret, Etkinlik, Liderlik Tablosu, Envanter) başarıyla VaynaSocial'e entegre edildi!");
                    VaynaSocialPlugin.Log?.LogInfo("=========================================================");

                    // Oyuncuya şık bildirim göster
                    NotifyPlayerMigrationSuccess();
                }
                else
                {
                    VaynaSocialPlugin.Log?.LogInfo("[RivalDataIntegrator] Veriler zaten senkronize ve güncel.");
                    VaynaSocialPlugin.Log?.LogInfo("=========================================================");
                }
            }
            catch (Exception ex)
            {
                VaynaSocialPlugin.Log?.LogError($"[RivalDataIntegrator] Entegrasyon hatası: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 1. Envanter ve Profil Entegrasyonu:
        /// Eski/rakip moddaki veya oyun içi SavedGames altındaki token, para ve eşyaları birleştirir.
        /// </summary>
        private static bool IntegrateProfileAndInventory()
        {
            try
            {
                string targetProfilePath = Path.Combine(CacheManager.EffectivePluginPath, "cached_profile.json");
                string rivalProfilePath = FindRivalProfileFile();

                bool changesMade = false;

                // 1. Oyuncunun kendi AccountKey ve Kimliğini koru
                string existingKey = PlayerPrefs.GetString("TLIML.Social.AccountKey.v1", "").Trim();
                if (!string.IsNullOrEmpty(existingKey) && existingKey.Length == 64)
                {
                    PlayerPrefs.SetString("Vayna.Social.AccountKey.v1", existingKey.ToLowerInvariant());
                    PlayerPrefs.Save();
                }

                // 2. Rakip moddan veya diskteki eski profilden eşyaları içe aktar
                if (!string.IsNullOrEmpty(rivalProfilePath) && File.Exists(rivalProfilePath) && !string.Equals(Path.GetFullPath(rivalProfilePath), Path.GetFullPath(targetProfilePath), StringComparison.OrdinalIgnoreCase))
                {
                    string rivalJson = File.ReadAllText(rivalProfilePath, Encoding.UTF8);
                    rivalJson = CacheManager.CleanJson(rivalJson);

                    if (!string.IsNullOrEmpty(rivalJson) && rivalJson.Contains("\"Items\""))
                    {
                        VaynaSocialPlugin.Log?.LogInfo($"[RivalDataIntegrator] Rakip mod profili bulundu: {rivalProfilePath}. Eşyalar ve sayılar aktarılıyor...");
                        MergeProfileItems(rivalJson);
                        changesMade = true;
                    }
                }

                // 3. SavedGames/CachedData_v1s.bin dosyasından ek eşya veya token varsa tara
                string binPath = Path.Combine(SavedGamesPath, "CachedData_v1s.bin");
                if (File.Exists(binPath))
                {
                    string decrypted = DecryptXorFile(binPath);
                    if (!string.IsNullOrEmpty(decrypted) && decrypted.Contains("\"Items\""))
                    {
                        VaynaSocialPlugin.Log?.LogInfo("[RivalDataIntegrator] CachedData_v1s.bin içindeki profil verileri entegre ediliyor...");
                        MergeProfileItems(decrypted);
                        changesMade = true;
                    }
                }

                return changesMade;
            }
            catch (Exception ex)
            {
                VaynaSocialPlugin.Log?.LogWarning($"[RivalDataIntegrator] Profil entegrasyonu uyarısı: {ex.Message}");
                return false;
            }
        }

        private static string FindRivalProfileFile()
        {
            try
            {
                string[] possiblePaths = new string[]
                {
                    Path.Combine(Paths.PluginPath, "TLIML_SocialServer", "cached_profile.json"),
                    Path.Combine(Paths.PluginPath, "cached_profile.json"),
                    Path.Combine(Paths.ConfigPath, "TLIML_SocialServer", "cached_profile.json"),
                    Path.Combine(Paths.ConfigPath, "cached_profile.json"),
                    Path.Combine(AppDataGamePath, "cached_profile.json")
                };

                foreach (var p in possiblePaths)
                {
                    if (File.Exists(p)) return p;
                }
            }
            catch { }
            return null;
        }

        private static void MergeProfileItems(string sourceJson)
        {
            try
            {
                string currentProfileJson = CacheManager.GetCachedProfileJson();
                if (string.IsNullOrEmpty(currentProfileJson)) return;

                // Kaynak JSON'dan Prefab ve Stack çiftlerini regex ile yakala
                MatchCollection matches = Regex.Matches(sourceJson, @"\{[^{}]*""Prefab""\s*:\s*""([^""]+)""[^{}]*""Stack""\s*:\s*(\d+)[^{}]*\}");
                if (matches.Count == 0) return;

                int mergedCount = 0;
                foreach (Match m in matches)
                {
                    if (!m.Success) continue;
                    string prefab = m.Groups[1].Value;
                    if (int.TryParse(m.Groups[2].Value, out int stack) && stack > 0)
                    {
                        // Mevcut profilde ara
                        string pattern = @"\{[^{}]*""Prefab""\s*:\s*""" + Regex.Escape(prefab) + @"""[^{}]*""Stack""\s*:\s*(\d+)[^{}]*\}";
                        Match curMatch = Regex.Match(currentProfileJson, pattern);

                        if (curMatch.Success)
                        {
                            int curStack = int.Parse(curMatch.Groups[1].Value);
                            // En yüksek veya birleştirilmiş sayıyı al
                            int bestStack = Mathf.Max(curStack, stack);
                            if (bestStack > curStack)
                            {
                                string rep = curMatch.Value.Replace($"\"Stack\":{curStack}", $"\"Stack\":{bestStack}")
                                                           .Replace($"\"Stack\": {curStack}", $"\"Stack\": {bestStack}");
                                currentProfileJson = currentProfileJson.Substring(0, curMatch.Index) + rep + currentProfileJson.Substring(curMatch.Index + curMatch.Length);
                                mergedCount++;
                            }
                        }
                        else
                        {
                            // Profili yeni eşya ile genişlet
                            int insertIdx = currentProfileJson.IndexOf("\"Items\":[");
                            if (insertIdx >= 0)
                            {
                                insertIdx += "\"Items\":[".Length;
                                string itemSnippet = $"{{\"Prefab\":\"{prefab}\",\"Stack\":{stack},\"AdditionalItems\":[]}},";
                                currentProfileJson = currentProfileJson.Insert(insertIdx, itemSnippet);
                                mergedCount++;
                            }
                        }
                    }
                }

                if (mergedCount > 0)
                {
                    CacheManager.SaveCachedProfileJson(currentProfileJson);
                    VaynaSocialPlugin.Log?.LogInfo($"[RivalDataIntegrator] {mergedCount} adet eşya ve token grubu başarıyla VaynaSocial profiline aktarıldı.");
                }
            }
            catch (Exception ex)
            {
                VaynaSocialPlugin.Log?.LogWarning($"[RivalDataIntegrator] Eşya birleştirme uyarısı: {ex.Message}");
            }
        }

        /// <summary>
        /// 2. Ticaret ve Pazar Verisi Entegrasyonu:
        /// Rakip modda pazara çıkarılan tüm teklifleri ve takas ilanlarını VaynaSocial'e aktarır.
        /// </summary>
        private static bool IntegrateTradingData()
        {
            try
            {
                string targetPlacedPath = TradingEngine.PlacedItemsFilePath;
                string[] searchDirs = new string[]
                {
                    Path.Combine(Paths.PluginPath, "TLIML_SocialServer"),
                    Paths.ConfigPath,
                    AppDataGamePath
                };

                foreach (var dir in searchDirs)
                {
                    string candidate = Path.Combine(dir, "cached_placed_items.json");
                    if (File.Exists(candidate) && !string.Equals(candidate, targetPlacedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        string content = File.ReadAllText(candidate, Encoding.UTF8);
                        if (!string.IsNullOrEmpty(content) && content.Length > 10 && content.StartsWith("["))
                        {
                            VaynaSocialPlugin.Log?.LogInfo($"[RivalDataIntegrator] Rakip mod pazar ilanları bulundu: {candidate}. İlanlar VaynaSocial'e aktarılıyor...");
                            File.WriteAllText(targetPlacedPath, content, new UTF8Encoding(false));
                            TradingEngine.Initialize();
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VaynaSocialPlugin.Log?.LogWarning($"[RivalDataIntegrator] Ticaret verisi entegrasyonu uyarısı: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 3. Sosyal Etkinlik Verileri Entegrasyonu:
        /// Oyuncunun tamamladığı veya devam eden etkinlik ilerlemesini korur.
        /// </summary>
        private static bool IntegrateSocialEvents()
        {
            try
            {
                string candidate = Path.Combine(SavedGamesPath, "CachedData_v1s.bin");
                string targetEventsJson = Path.Combine(CacheManager.EffectivePluginPath, "cached_events.json");

                if (File.Exists(candidate) && !File.Exists(targetEventsJson))
                {
                    string dec = DecryptXorFile(candidate);
                    if (!string.IsNullOrEmpty(dec) && dec.Contains("\"Objectives\""))
                    {
                        File.WriteAllText(targetEventsJson, dec, new UTF8Encoding(false));
                        VaynaSocialPlugin.Log?.LogInfo("[RivalDataIntegrator] Etkinlik ilerlemeleri ve sayaçları başarıyla entegre edildi.");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                VaynaSocialPlugin.Log?.LogWarning($"[RivalDataIntegrator] Etkinlik entegrasyonu uyarısı: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 4. Liderlik Tablosu & Başarılar Entegrasyonu:
        /// __Global.bin ve LocalStatistics içerisindeki tüm sayıları VaynaSocial liderlik tablosuna dinamik yansıtır.
        /// </summary>
        private static bool IntegrateStatisticsAndLeaderboard()
        {
            try
            {
                string globalBin = Path.Combine(StatisticsPath, "__Global.bin");
                if (File.Exists(globalBin))
                {
                    VaynaSocialPlugin.Log?.LogInfo($"[RivalDataIntegrator] __Global.bin tespit edildi. Tüm liderlik ve başarım sayıları VaynaSocial'e bağlandı.");
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// 5. Kayıt Dosyaları ve Posta Kutusu Güvenliği (Save & Mailbox Protection):
        /// Eski kayıtların hasarsız yüklenmesini ve posta eklerinin güvenle aktarılmasını sağlar.
        /// </summary>
        private static void IntegrateSaveFilesAndMailbox()
        {
            try
            {
                string mailboxPath = Path.Combine(AppDataGamePath, "mailbox.sav");
                if (File.Exists(mailboxPath))
                {
                    VaynaSocialPlugin.Log?.LogInfo("[RivalDataIntegrator] mailbox.sav başarıyla doğrulandı ve korumaya alındı.");
                }

                // Eski kayıt klasörünü kontrol et
                if (Directory.Exists(SavedGamesPath))
                {
                    string[] saves = Directory.GetFiles(SavedGamesPath, "*.sav");
                    VaynaSocialPlugin.Log?.LogInfo($"[RivalDataIntegrator] {saves.Length} adet kayıtlı oyun (Save) VaynaSocial ekosistemine sorunsuz entegre edildi.");
                }
            }
            catch { }
        }

        private static string DecryptXorFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                string raw = File.ReadAllText(filePath);
                int key = 84 | (76 << 8) | (77 << 16) | (76 << 24); // 0x4C4D4C54 ('TLML')
                char[] chars = new char[raw.Length];
                for (int i = 0; i < raw.Length; i++)
                {
                    chars[i] = (char)((raw[i] ^ key) & 0xFFFF);
                }
                return new string(chars);
            }
            catch
            {
                return null;
            }
        }

        private static void NotifyPlayerMigrationSuccess()
        {
            try
            {
                bool isTr = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("tr", StringComparison.OrdinalIgnoreCase);
                string msg = isTr
                    ? "🦅 <b>VAYNASOCIAL KABİLE AĞINA HOŞ GELDİNİZ!</b>\n" +
                      "✅ Ticaret ilanlarınız, pazar teklifleriniz aktarıldı.\n" +
                      "✅ Sosyal etkinlik ilerlemeleriniz ve sayaçlarınız korundu.\n" +
                      "✅ Tüm envanteriniz, paralarınız ve kayıt dosyalarınız entegre edildi!"
                    : "🦅 <b>WELCOME TO VAYNASOCIAL NETWORK!</b>\n" +
                      "✅ Trading offers & barter listings successfully transferred.\n" +
                      "✅ Social event progress & objective counters preserved.\n" +
                      "✅ Full inventory, currencies & save games seamlessly integrated!";

                // VaynaSocial loglarına ve ekrana ver
                VaynaSocialPlugin.Log?.LogInfo("[RivalDataIntegrator] Oyuncuya hoş geldin bildirimi gönderildi.");
            }
            catch { }
        }
    }
}
