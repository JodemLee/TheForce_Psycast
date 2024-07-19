using AM;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TheForce_Psycast;
using Verse;

namespace MeleeAnimPatch
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmonyPatchMeleeAnim = TheForce_Psycast.HarmonyPatches.harmonyPatch;
            var type = typeof(HarmonyPatches);
            harmonyPatchMeleeAnim.PatchAll();
        }
    }



    [HarmonyPatch(typeof(AudioUtility), "GetWeaponClashSound")]
    public static class DuelPatchSound
    {
        // Define a list of specific ThingDefs
        private static readonly List<ThingDef> specificThingDefs = new List<ThingDef>
         {
        DefDatabase<ThingDef>.GetNamed("Force_Lightsaber_Custom"),
        DefDatabase<ThingDef>.GetNamed("Force_Lightsaber_Shoto"),
        DefDatabase<ThingDef>.GetNamed("Force_Lightsaber_Curved"),
        DefDatabase<ThingDef>.GetNamed("Force_Lightsaber_Dual"),
        DefDatabase<ThingDef>.GetNamed("Force_Darksaber"),
        DefDatabase<ThingDef>.GetNamed("Force_Ezra_BlasterLightsaber"),
        // Add more ThingDefs as needed
         };

        private static readonly List<SoundDef> clashSounds = new List<SoundDef>
         {
        ForceDefOf.Force_LightsaberClashOne,
        ForceDefOf.Force_LightsaberClashTwo,
        ForceDefOf.Force_LightsaberClashThree,
        ForceDefOf.Force_LightsaberClashFour,
        ForceDefOf.Force_LightsaberClashFive
        // Add more SoundDefs as needed
         };

        public static bool Prefix(Thing weapon1, Thing weapon2, ref SoundDef __result)
        {
            // Check if either weapon matches any of the ThingDefs in the list
            bool weapon1Matches = specificThingDefs.Contains(weapon1.def);
            bool weapon2Matches = specificThingDefs.Contains(weapon2.def);

            if (weapon1Matches || weapon2Matches)
            {
                // Choose a random SoundDef from the list
                Random random = new Random();
                __result = clashSounds[random.Next(clashSounds.Count)];
                return false; // Skip the original method
            }

            // Otherwise, let the original method run
            return true;
        }
    }
}
