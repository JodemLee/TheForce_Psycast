using HarmonyLib;
using ModularWeapons;
using System.Linq;
using UnityEngine;
using Verse;
using ModularWeapon_Patch;
using System.Collections.Generic;
using TheForce_Psycast.Lightsabers;
using TheForce_Psycast;

namespace ModularWeapon_Patch
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("ModularWeapon_Psycast_ForceThe");
            var type = typeof(HarmonyPatches);
            harmony.Patch(AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming)),
               prefix: new HarmonyMethod(type, nameof(DrawEquipmentAimingModularFix)));
            harmony.PatchAll();

        }

        public static Harmony harmonyPatch;
        public static bool DrawEquipmentAimingModularFix(Thing eq, Vector3 drawLoc, float aimAngle)
        {

            if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Melee Animation"))
            {
                return true;
            }

            if (eq == null)
            {
                return true;
            }

            if (eq.def == null)
            {
                return true;
            }

            if (eq.def.graphicData == null)
            {
                return true;
            }

            var compLightsaberBlade = eq.TryGetComp<Comp_WeaponStance>();

            if (compLightsaberBlade != null)
            {
                bool flip = false;
                float angle = aimAngle - 90f;

                if (aimAngle > 20f && aimAngle < 160f)
                {
                    angle += compLightsaberBlade.CurrentRotation;
                }
                else if (aimAngle > 200f && aimAngle < 340f)
                {
                    flip = false;
                    angle -= 180f + compLightsaberBlade.CurrentRotation;
                }
                else
                {
                    angle += compLightsaberBlade.CurrentRotation;
                }
                angle = compLightsaberBlade.CurrentRotation;

                if (compLightsaberBlade.IsAnimatingNow && Force_ModSettings.DeflectionSpin)
                {
                    float animationTicks = compLightsaberBlade.AnimationDeflectionTicks;

                    if (!Find.TickManager.Paused && compLightsaberBlade.IsAnimatingNow)
                    {
                        compLightsaberBlade.AnimationDeflectionTicks -= 20;
                    }

                    if (animationTicks > 0)
                    {
                        if (flip)
                        {
                            angle -= (animationTicks + 1) / 2;
                        }
                        else
                        {
                            angle += (animationTicks + 1) / 2;
                        }
                    }
                }

                angle %= 360f;
                Vector3 offset = compLightsaberBlade.CurrentDrawOffset;
                var matSingle = ModularWeaponUtility.GetInnerGrapgic_Modular(eq).MatSingleFor(eq);
                var s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);

                var matrix = Matrix4x4.TRS(drawLoc + offset, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(flip ? MeshPool.plane10Flip : MeshPool.plane10, matrix, matSingle, 0);

                return false;
            }
            return true;
        }



        [HarmonyPatch(typeof(Hediff_LightsaberDeflection), "GetGizmos")]
        public static class Hediff_LightsaberDeflection_GetGizmos_Patch
        {
            public static bool Prefix(Hediff_LightsaberDeflection __instance, ref IEnumerable<Gizmo> __result)
            {
                // Log the pawn and equipment state
                if (__instance.pawn == null || __instance.pawn.equipment == null || __instance.pawn.equipment.Primary == null)
                {
                    return true; // Continue to the original method
                }

                // Log the ThingDef of the primary weapon
                ThingDef primaryDef = __instance.pawn.equipment.Primary.def;
                if (primaryDef == null)
                {
                    return true; // Continue to the original method
                }

                // Check if the primary weapon has a CompProperties_WeaponStance
                var compLightsaber = primaryDef.GetCompProperties<CompProperties_WeaponStance>();
                if (compLightsaber == null)
                {
                    return true; // Continue to the original method if the component is not present
                }

                // Create a new list for the gizmos
                List<Gizmo> gizmos = new List<Gizmo>();

                // Add the custom weapon stance gizmo
                gizmos.Add(new Gizmo_WeaponStance(__instance.pawn, __instance, primaryDef));

                // Set the result to the modified gizmo list and skip the original method
                __result = gizmos;
                return false; // Skip the original method
            }
        }
    }
}