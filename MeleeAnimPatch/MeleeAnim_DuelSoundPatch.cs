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


    public class LightsaberClashModExtension : DefModExtension
    {
        public List<SoundDef> clashSounds; // The list of sounds to play for this lightsaber type
    }


    [HarmonyPatch(typeof(AudioUtility), "GetWeaponClashSound")]
    public static class DuelPatchSound
    {
        public static bool Prefix(Thing weapon1, Thing weapon2, ref SoundDef __result)
        {
            // Check if the first weapon has a LightsaberClashModExtension
            var weapon1ModExt = weapon1.def.GetModExtension<LightsaberClashModExtension>();
            var weapon2ModExt = weapon2.def.GetModExtension<LightsaberClashModExtension>();

            // If either weapon has the mod extension, choose a sound from its defined list
            if (weapon1ModExt != null || weapon2ModExt != null)
            {
                var soundList = weapon1ModExt?.clashSounds ?? weapon2ModExt?.clashSounds;
                if (soundList != null && soundList.Count > 0)
                {
                    Random random = new Random();
                    __result = soundList[random.Next(soundList.Count)];
                    return false; // Skip the original method
                }
            }

            // Let the original method run if no mod extension is found
            return true;
        }
    }
}
