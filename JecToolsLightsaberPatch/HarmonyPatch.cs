using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using CompActivatableEffect;

namespace JecToolsLightsaberPatch
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            harmonyPatch = new Harmony("lightsaberjec_leepatch");
            var type = typeof(HarmonyPatches);
            harmonyPatch.PatchAll();
            var targetType = AccessTools.TypeByName("CompActivatableEffect.HarmonyCompActivatableEffect");
            var method = AccessTools.Method(targetType, "DrawEquipmentAimingPreFix");

            // Apply the prefix
            harmonyPatch.Patch(method, prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DrawEquipmentAimingPostFix_Prefix)));
        }
        public static Harmony harmonyPatch;

        public static bool DrawEquipmentAimingPostFix_Prefix()
        {
            // Check if the "Melee Animation" mod is active
            if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Melee Animation"))
            {
                // If the mod is loaded, return false to skip the original method
                return false;
            }

            // Continue with the original method if the mod is not loaded
            return true;
        }
    }
}