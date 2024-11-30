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
            // Log entry into the patch method
            Log.Message($"DuelPatchSound: Checking clash sound for weapons {weapon1.def.defName} and {weapon2.def.defName}");

            // Check if the first weapon has a LightsaberClashModExtension
            var weapon1ModExt = weapon1.def.GetModExtension<LightsaberClashModExtension>();
            var weapon2ModExt = weapon2.def.GetModExtension<LightsaberClashModExtension>();

            // Log whether mod extensions were found on either weapon
            if (weapon1ModExt != null)
                Log.Message($"DuelPatchSound: Found mod extension on weapon1 ({weapon1.def.defName})");

            if (weapon2ModExt != null)
                Log.Message($"DuelPatchSound: Found mod extension on weapon2 ({weapon2.def.defName})");

            // If either weapon has the mod extension, choose a sound from its defined list
            if (weapon1ModExt != null || weapon2ModExt != null)
            {
                var soundList = weapon1ModExt?.clashSounds ?? weapon2ModExt?.clashSounds;
                if (soundList != null && soundList.Count > 0)
                {
                    Random random = new Random();
                    __result = soundList[random.Next(soundList.Count)];

                    // Log the selected sound
                    Log.Message($"DuelPatchSound: Selected clash sound {__result.defName}");
                    return false; // Skip the original method
                }
                else
                {
                    Log.Message("DuelPatchSound: No clash sounds defined in the mod extension.");
                }
            }
            else
            {
                Log.Message("DuelPatchSound: No mod extensions found on either weapon.");
            }

            // Let the original method run if no mod extension is found
            return true;
        }
    }
}
