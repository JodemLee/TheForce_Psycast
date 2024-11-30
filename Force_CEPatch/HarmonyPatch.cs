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

        [HarmonyPatch(typeof(BulletCE), "Impact", new Type[] { typeof(Thing) })]
        public static class BulletCE_Impact
        {
            public static bool Prefix(Thing hitThing, BulletCE __instance, ref Thing ___launcher, ref Thing ___intendedTarget, ref Ray ___shotLine, ref float ___shotRotation, ref Vector2 ___origin, ref bool ___landed)
            {
                if (!(hitThing is Pawn pawn) || pawn.kindDef.RaceProps.IsMechanoid )
                {
                    return true;
                }
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance) as Hediff_LightsaberDeflectionCE;
                if (hediff == null)
                {
                    return true;
                }
                Effecter effecter = new Effecter(EffecterDefOf.Interceptor_BlockedProjectile);
                Traverse.Create(pawn).Field("drawer").GetValue<Pawn_DrawTracker>()
            .Notify_DamageDeflected(new DamageInfo(((Thing)(object)__instance).def.projectile.damageDef, (__instance).def.projectile.damageDef.defaultDamage));
                effecter.Trigger(new TargetInfo(__instance.Position, pawn.Map), TargetInfo.Invalid);
                effecter.Cleanup();
                if (hediff.ShouldDeflectProjectile(__instance))
                {
                    ___intendedTarget = ___launcher;
                    ___launcher = hitThing;
                    ___shotRotation = (___shotRotation + 180f) % 360f;
                    ___shotLine = new Ray(___shotLine.direction, ___shotLine.origin);
                    ((ProjectileCE)__instance).Destination = ___origin;
                    ___origin = new Vector2(((Thing)(object)__instance).Position.x, ((Thing)(object)__instance).Position.z);
                    ___landed = false;
                    hediff.AddEntropy(__instance);
                }
                return false;
            }
        }
    }
}
