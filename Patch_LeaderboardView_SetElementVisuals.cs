using System;
using HarmonyLib;
using UnityEngine;

namespace VaynaSocial
{
    [HarmonyPatch(typeof(LeaderboardView), "SetElementVisuals")]
    internal static class Patch_LeaderboardView_SetElementVisuals
    {
        private static void Prefix(int elementIdx, ref string positionString, ref string nameString, ref string scoreString, ref int rank, ref bool highlight)
        {
            if (string.IsNullOrEmpty(nameString)) return;

            if (string.Equals(nameString, VaynaSocialPlugin.GetPlayerDisplayName(), StringComparison.OrdinalIgnoreCase))
            {
                highlight = true;
            }
        }
    }
}
