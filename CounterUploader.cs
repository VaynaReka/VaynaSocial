// Mod yapimcisi: Vayna Reka
// Bagimlilik: Bu mod VaynaCore.dll'ye baglidir (BepInEx/plugins icinde bulunmali).

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

namespace VaynaSocial
{
    public class CounterUploader : MonoBehaviour
    {
        private const float FlushInterval = 60f;

        private static readonly Dictionary<string, long> pending = new Dictionary<string, long>();
        private static readonly object pendingLock = new object();
        private bool sending;

        // Yüzde olarak gösterilen sayaçlar (doğruluk gibi) toplanamaz; gönderilmez.
        public static void Add(string counterId, int amount)
        {
            if (string.IsNullOrEmpty(counterId) || amount <= 0) return;
            try
            {
                var settings = StatCountersManager.Instance?.SettingsBundle?.Settings?.Find(x => x.Id == counterId);
                if (settings != null && settings.DisplayValueAsPercent) return;
            }
            catch { }

            lock (pendingLock)
            {
                long cur;
                pending.TryGetValue(counterId, out cur);
                pending[counterId] = cur + amount;
            }
        }

        private void Start()
        {
            base.StartCoroutine(this.FlushLoop());
        }

        private IEnumerator FlushLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(FlushInterval);
                if (!this.sending && ModConfig.EnableSocial.Value)
                {
                    yield return base.StartCoroutine(this.FlushOnce());
                }
            }
        }

        private IEnumerator FlushOnce()
        {
            Dictionary<string, long> batch;
            lock (pendingLock)
            {
                if (pending.Count == 0) yield break;
                batch = new Dictionary<string, long>(pending);
                pending.Clear();
            }

            this.sending = true;

            var counters = new JsonData();
            counters.SetJsonType(JsonType.Object);
            foreach (var kv in batch)
            {
                counters[kv.Key] = kv.Value;
            }
            var root = new JsonData();
            root["Name"] = VaynaSocialPlugin.GetPlayerDisplayName();
            root["Counters"] = counters;

            string url = ModConfig.SocialServerUrl.Value.TrimEnd('/') + "/LeaderboardsService/AddCounters/";
            bool ok = false;

            using (var www = new UnityWebRequest(url, "POST"))
            {
                www.timeout = 10;
                www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(root.ToJson())) { contentType = "application/json" };
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("OnlineTicket", VaynaSocialPlugin.CachedAccountKey);

                yield return www.SendWebRequest();

                ok = !www.isNetworkError && !www.isHttpError;
                if (!ok && ModConfig.DebugLog.Value)
                {
                    VaynaSocialPlugin.Log?.LogWarning("[VaynaSocial] Sayaç gönderilemedi: " + www.error);
                }
            }

            if (!ok)
            {
                // Başarısızsa geri koy, sonraki turda tekrar denenir
                lock (pendingLock)
                {
                    foreach (var kv in batch)
                    {
                        long cur;
                        pending.TryGetValue(kv.Key, out cur);
                        pending[kv.Key] = cur + kv.Value;
                    }
                }
            }

            this.sending = false;
        }
    }

    // Oyunun sayaç artışını yakalar (her sayaç için bir kez çağrılır)
    [HarmonyPatch(typeof(SocialEventsManager), "OnCounterIncreased")]
    internal static class Patch_SocialEventsManager_OnCounterIncreased
    {
        [HarmonyPrefix]
        private static void Prefix(SocialEventsManager __instance, string counterId, int amount)
        {
            if (__instance == null || __instance.Events == null || string.IsNullOrEmpty(counterId))
            {
                return;
            }

            foreach (var ev in __instance.Events)
            {
                if (ev == null || ev.ObjectivesTrackers == null) continue;
                for (int i = 0; i < ev.ObjectivesTrackers.Count; i++)
                {
                    var tracker = ev.ObjectivesTrackers[i];
                    if (tracker == null) continue;

                    if (!string.IsNullOrEmpty(tracker.CounterId) && tracker.CounterId.Contains(";"))
                    {
                        int semi = tracker.CounterId.IndexOf(';');
                        string pure = tracker.CounterId.Substring(0, semi);
                        if (pure.Equals(counterId, StringComparison.OrdinalIgnoreCase))
                        {
                            tracker.CounterId = pure;
                        }
                    }

                    if (tracker.TargetVal <= 0)
                    {
                        int qMin = (ev.Settings != null && ev.Settings.Objectives != null && i < ev.Settings.Objectives.Count)
                            ? ev.Settings.Objectives[i].QuantityMin : 3;
                        string eventId = ev.Settings != null ? (ev.Settings.Id ?? "") : "";
                        tracker.TargetVal = Patch_SocialEventCard_UpdateVisuals.GetEventTargetCount(eventId, tracker.CounterId, qMin);
                        if (tracker.TargetVal <= 0) tracker.TargetVal = 3;
                    }
                }
            }
        }

        [HarmonyPostfix]
        private static void Postfix(string counterId, int amount)
        {
            CounterUploader.Add(counterId, amount);
        }
    }
}
