using HarmonyLib;
using System;

namespace VaynaSocial
{
    /// <summary>
    /// Oyundaki kamp ateşi dinlenme/kayıt ve haritadan hızlı seyahat (fast travel)
    /// yaparken ortaya çıkan yapay ve gereksiz 3-5 saniyelik siyah bekleme ekranını
    /// eğer oyuncu config'de EnableInstantTravelAndRest = true yapmışsa anında geçer.
    /// Varsayılan olarak 'false' tutulur böylece orijinal atmosferik dinlenme bozulmaz.
    /// Dinlenme veya hızlı seyahat bittiğinde dünyadaki tüm eski cesetleri süpürür.
    /// </summary>
    [HarmonyPatch(typeof(LoadingScreen), "Show", new Type[] { typeof(float), typeof(string) })]
    public static class Patch_LoadingScreen_Show
    {
        public static void Prefix(ref float minDuration, string text)
        {
            if (ModConfig.EnableInstantTravelAndRest == null || !ModConfig.EnableInstantTravelAndRest.Value)
            {
                return;
            }

            float targetDuration = ModConfig.InstantRestDuration != null ? ModConfig.InstantRestDuration.Value : 0.05f;
            if (minDuration > targetDuration && minDuration < 1000f)
            {
                minDuration = targetDuration;
            }
        }
    }

    [HarmonyPatch(typeof(LoadingScreen), "Hide", new Type[] { typeof(int), typeof(float), typeof(float) })]
    public static class Patch_LoadingScreen_Hide
    {
        public static void Prefix(ref int framesOffset, ref float offset, ref float hideDuration)
        {
            try
            {
                // Yalnizca normal oynanis esnasinda calis (yeni oyunda veya yukleme sirasinda calisma)
                if (SandboxFlow.Instance == null && PlayerActions.Instance != null && PlayerActions.Instance.entity != null)
                {
                    CorpseCleanupManager.CleanupAllDeadBodies(forceAll: true);
                }
            }
            catch {}

            if (ModConfig.EnableInstantTravelAndRest == null || !ModConfig.EnableInstantTravelAndRest.Value)
            {
                return;
            }

            if (framesOffset > 1) framesOffset = 0;
            if (offset > 0.05f) offset = 0.02f;
            if (hideDuration > 0.1f) hideDuration = 0.08f;
        }
    }
}
