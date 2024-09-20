using CombatExtended;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast;
using TheForce_Psycast.Abilities.Darkside;
using TheForce_Psycast.Lightsabers;
using TheForce_Psycast.Lightsabers.Modular_Weapon;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Force_CEPatch
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            harmony = new Harmony("Psycast_ForceThe");
            var type = typeof(HarmonyPatches);
            harmony.PatchAll();
            harmony.Patch(AccessTools.Method(typeof(ProjectileCE), nameof(ProjectileCE.Tick)),
               postfix: new HarmonyMethod(type, nameof(Postfix)));
        }

        public static Harmony harmony;

        public static void Postfix(ProjectileCE __instance)
        {
            if (__instance.globalTargetInfo.IsValid)
            {
                Pawn pawn = (Pawn)__instance.intendedTarget;
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance) as Hediff_LightsaberDeflectionCE;
                if (pawn != null && hediff.ShouldDeflectProjectile(__instance))
                {
                    hediff.DeflectProjectile(__instance);
                }
            }
        }
    }
}
