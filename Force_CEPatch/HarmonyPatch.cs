using CombatExtended;
using HarmonyLib;
using RimWorld;
using System;
using TheForce_Psycast;
using UnityEngine;
using Verse;

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
        }

        public static Harmony harmony;

        [HarmonyPatch(typeof(BulletCE), nameof(ProjectileCE.Impact), typeof(Thing))]
        public static class BulletCE_Impact
        {
            public static bool Prefix(Thing hitThing, BulletCE __instance, ref Thing ___launcher, ref Thing ___intendedTarget, ref Ray ___shotLine, ref float ___shotRotation, ref Vector2 ___origin, ref bool ___landed)
            {
                if (!(hitThing is Pawn pawn))
                {
                    return true;
                }
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance) as Hediff_LightsaberDeflectionCE;
                if (hediff == null)
                {
                    return true;
                }

                if (!hediff.ShouldDeflectProjectile(__instance))
                {
                    return true;
                }

                Effecter effecter = new Effecter(EffecterDefOf.Interceptor_BlockedProjectile);

                effecter.Trigger(new TargetInfo(__instance.Position, pawn.Map), TargetInfo.Invalid);

                var value2 = Traverse.Create(pawn).Field("drawer").GetValue<Pawn_DrawTracker>();
                value2.Notify_DamageDeflected(new DamageInfo(__instance.def.projectile.damageDef, 1f));

                effecter.Cleanup();

                if (___launcher != null)
                {
                    ___intendedTarget = ___launcher;
                    ___launcher = hitThing;
                    ___shotRotation = (___shotRotation + 180) % 360;
                    ___shotLine = new Ray(___shotLine.direction, ___shotLine.origin);
                    __instance.Destination = ___origin;
                    ___origin = new Vector2(__instance.Position.x, __instance.Position.z);
                    ___landed = false;
                    hediff.AddEntropy(__instance);
                }

                else
                {
                    __instance.Destroy();

                }
                return false;
            }
        }
    }
}
