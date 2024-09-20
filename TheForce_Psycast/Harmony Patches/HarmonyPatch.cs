using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast.Abilities.Darkside;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;


namespace TheForce_Psycast
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            harmonyPatch = new Harmony("Psycast_ForceThe");
            var type = typeof(HarmonyPatches);
            harmonyPatch.Patch(AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming)),
               prefix: new HarmonyMethod(type, nameof(DrawEquipmentAimingPreFix)));
            harmonyPatch.PatchAll();
        }

        public static Harmony harmonyPatch;

        public static bool DrawEquipmentAimingPreFix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Melee Animation"))
            {
                return true;
            }

            if (eq == null || eq.def == null || eq.def.graphicData == null)
            {
                return true;
            }

            var compLightsaberBlade = eq?.TryGetComp<Comp_LightsaberBlade>();

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
                        compLightsaberBlade.AnimationDeflectionTicks -= 20;

                    // Lerp from current angle to lastInterceptAngle for smooth rotation
                    float targetAngle = compLightsaberBlade.lastInterceptAngle;
                    angle = Mathf.Lerp(angle, targetAngle, 0.1f); // Smooth transition

                    if (animationTicks > 0)
                    {
                        // Optional: flip animation if required
                        if (flip)
                            angle -= (animationTicks + 1) / 2;
                        else
                            angle += (animationTicks + 1) / 2;
                    }
                }


                angle %= 360f;
                Vector3 offset = compLightsaberBlade.CurrentDrawOffset;
                DrawLightsaberGraphics(eq, drawLoc + offset, angle, flip, compLightsaberBlade);

                var matSingle = eq.Graphic.MatSingle;
                var s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                var matrix = Matrix4x4.TRS(drawLoc + offset, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(flip ? MeshPool.plane10Flip : MeshPool.plane10, matrix, matSingle, 0);

                return false;
            }

            return true;
        }
        private static void DrawLightsaberGraphics(Thing eq, Vector3 drawLoc, float angle, bool flip, Comp_LightsaberBlade compLightsaberBlade)
        {
            // Offset and scale settings for the lightsaber components
            float coreYOffset = -0.001f;
            Vector3 coreDrawLoc = drawLoc;
            coreDrawLoc.y += coreYOffset;

            float bladeYOffset = -0.002f;
            Vector3 bladeDrawLoc = drawLoc;
            bladeDrawLoc.y += bladeYOffset;

            Vector3 scale = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
            Matrix4x4 bladeMatrix = Matrix4x4.TRS(bladeDrawLoc, Quaternion.AngleAxis(angle, Vector3.up), scale);
            Matrix4x4 coreMatrix = Matrix4x4.TRS(coreDrawLoc, Quaternion.AngleAxis(angle, Vector3.up), scale);
            Matrix4x4 glowMatrix = Matrix4x4.TRS(coreDrawLoc, Quaternion.AngleAxis(angle, Vector3.up), Force_ModSettings.glowRadius * scale * 1.3f);

            // Draw the blade and core meshes
            if (compLightsaberBlade.Graphic != null)
            {
                DrawMesh(compLightsaberBlade.Graphic, bladeMatrix, flip, compLightsaberBlade.Props.Altitude);
            }

            if (compLightsaberBlade.LightsaberCore1Graphic != null)
            {
                DrawMesh(compLightsaberBlade.LightsaberCore1Graphic, coreMatrix, flip, compLightsaberBlade.Props.Altitude);
            }

            if (compLightsaberBlade.LightsaberBlade2Graphic != null)
            {
                DrawMesh(compLightsaberBlade.LightsaberBlade2Graphic, bladeMatrix, flip, compLightsaberBlade.Props.Altitude);
            }

            if (compLightsaberBlade.LightsaberCore2Graphic != null)
            {
                DrawMesh(compLightsaberBlade.LightsaberCore2Graphic, coreMatrix, flip, compLightsaberBlade.Props.Altitude);
            }

            if (compLightsaberBlade.LightsaberGlowGraphic != null && Force_ModSettings.LightsaberFakeGlow)
            {
                DrawMesh(compLightsaberBlade.LightsaberGlowGraphic, glowMatrix, flip, compLightsaberBlade.Props.Altitude);
            }
        }

        private static void DrawMesh(Graphic graphic, Matrix4x4 matrix, bool flip, float altitude)
        {
            if (graphic != null)
            {
                Material material = graphic.MatSingle;
                Graphics.DrawMesh(flip ? MeshPool.plane10Flip : MeshPool.plane10, matrix, material, (int)altitude);
            }
        }

        [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.CarryWeaponOpenly))]
        internal static class PawnRenderUtility_CarryWeaponOpenly_Postfix
        {
            /// <summary>
            /// Prevents the gun of the pawn from drawing if the pawn is throwing their lightsaber
            /// </summary>
            [HarmonyPostfix]
            static void HideLightsaberWhenThrown(ref bool __result, Pawn pawn)
            {
                if (!__result) return;

                var lightsabercomp = pawn.equipment?.Primary?.TryGetComp<Comp_LightsaberBlade>();

                if (lightsabercomp != null && lightsabercomp.IsThrowingWeapon
                    && !pawn.IsAttacking())
                {
                    __result = false;
                }
            }
        }
    }


    [HarmonyPatch(typeof(Projectile), "ImpactSomething")]
    public static class Patch_Projectile_ImpactSomething
    {
        public static bool Prefix(Projectile __instance)
        {
            // Check if the projectile is about to impact a pawn
            if (__instance.usedTarget.Thing is Pawn pawn)
            {
                // Find the Hediff_LightsaberDeflection instance
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance) as Hediff_LightsaberDeflection;
                if (hediff != null)
                {

                    // Use precise deflection logic to ensure only projectiles on a direct collision course are deflected
                    if (hediff.ShouldDeflectProjectile(__instance))
                    {
                        // Call the deflection method
                        hediff.DeflectProjectile(__instance);
                        return false;
                    }
                }

                // Optional: Add specific behavior for subclasses of Projectile
                if (__instance is ForceLightningProjectile)
                {
                    if (hediff != null)
                    {

                        // Use precise deflection logic to ensure only projectiles on a direct collision course are deflected
                        if (hediff.ShouldDeflectProjectile(__instance))
                        {
                            // Call the deflection method
                            hediff.DeflectProjectile(__instance);
                            return false;
                        }
                    }
                }
            }

            // Allow normal impact if deflection doesn't happen
            return true;
        }
    }

    [HarmonyPatch(typeof(Ideo), "SetIcon")]
    public static class Patch_Ideo_SetIcon
    {
        [HarmonyPostfix]
        public static void PostFix(Ideo __instance)
        {
            if (__instance.culture != null && __instance.culture.HasModExtension<DefCultureExtension>())
            {
                DefCultureExtension ext = __instance.culture.GetModExtension<DefCultureExtension>();

                if (ext.ideoIconDef != null)
                {
                    __instance.iconDef = ext.ideoIconDef;
                }
                if (ext.ideoIconColor != null)
                {
                    __instance.colorDef = ext.ideoIconColor;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn) })]
    public static class Patch_Thing_TrySpawnYield
    {
        [HarmonyPostfix]
        public static void Postfix(Mineable __instance, Map map, bool moteOnWaste, Pawn pawn)
        {

            var colorCrystalComp = __instance.TryGetComp<CompColorCrystal>();
            if (colorCrystalComp != null)
            {
                IntVec3 position = __instance.Position;
                foreach (Thing thing in position.GetThingList(map))
                {
                    var itemColorable = thing.TryGetComp<CompColorable>();
                    if (itemColorable != null)
                    {
                        itemColorable.SetColor(colorCrystalComp.CrystalColor.ToColor);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class MakeRecipeProducts_Patch
    {
        [HarmonyPostfix]
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, IBillGiver billGiver, Precept_ThingStyle precept = null, ThingStyleDef style = null, int? overrideGraphicIndex = null)
        {
            Thing kyberCrystal = ingredients.Find(ingredient => ingredient.def.defName == "Force_KyberCrystal");

            if (kyberCrystal != null && recipeDef.useIngredientsForColor)
            {
                Color crystalColor = kyberCrystal.DrawColor;
                foreach (var product in __result)
                {
                    var comp = product.TryGetComp<Comp_LightsaberBlade>();
                    if (comp != null)
                    {
                        comp.SetColor(crystalColor);
                        comp.SetLightsaberBlade2Color(crystalColor);
                    }
                    yield return product;
                }
            }
            else
            {
                foreach (var product in __result)
                {
                    yield return product;
                }
            }
        }
    }
}