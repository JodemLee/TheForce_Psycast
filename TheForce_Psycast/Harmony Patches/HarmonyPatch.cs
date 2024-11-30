using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;
using static TheForce_Psycast.Lightsabers.LightsaberGraphicsUtil;



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
            meleeAnimationModActive = ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Melee Animation");
        }

        public static Harmony harmonyPatch;

        private static bool meleeAnimationModActive;
        private static CompCache compCache = new CompCache();
        private static CompCacheGraphicCustomization compCacheGraphic = new CompCacheGraphicCustomization();

        public static bool DrawEquipmentAimingPreFix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            var eqPrimary = eq;
            if (meleeAnimationModActive || eqPrimary == null || eqPrimary.def?.graphicData == null)
            {
                return true;
            }

            var compLightsaberBlade = compCache.GetCachedComp(eqPrimary);
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

                if (compLightsaberBlade.IsAnimatingNow)
                {
                    float animationTicks = compLightsaberBlade.AnimationDeflectionTicks;
                    if (!Find.TickManager.Paused && compLightsaberBlade.IsAnimatingNow)
                        compLightsaberBlade.AnimationDeflectionTicks -= 20;
                    float targetAngle = compLightsaberBlade.lastInterceptAngle;
                    angle = Mathf.Lerp(angle, targetAngle, 0.1f);

                    if (animationTicks > 0)
                    {
                        if (flip)
                            angle -= (animationTicks + 1) / 2;
                        else
                            angle += (animationTicks + 1) / 2;
                    }
                }
                angle %= 360f;
                Vector3 offset = compLightsaberBlade.CurrentDrawOffset;
                DrawLightsaberGraphics(eqPrimary, drawLoc + offset, angle, flip, compLightsaberBlade);

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.CarryWeaponOpenly))]
        internal static class PawnRenderUtility_CarryWeaponOpenly_Postfix
        {
            [HarmonyPostfix]
            static void HideLightsaberWhenThrown(ref bool __result, Pawn pawn)
            {
                // Short-circuit if result is already false
                if (!__result) return;

                // Cache primary equipment
                var primaryEquipment = pawn.equipment?.Primary;
                if (primaryEquipment == null) return;

                // Cache lightsaber component
                var compLightsaberBlade = compCache.GetCachedComp(primaryEquipment);

                // Check if lightsaber is throwing weapon
                if (compLightsaberBlade?.IsThrowingWeapon == true)
                {
                    compLightsaberBlade.ResetToZero();
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.CarryWeaponOpenly))]
        internal static class PawnRenderUtility_CarryWeaponOpenly_PostfixIgnition
        {
            [HarmonyPostfix]
            static void IgniteLightsaberWhenDeflecting(ref bool __result, Pawn pawn)
            {
                // Short-circuit if result is already false
                if (__result) return;
                var primaryEquipment = pawn.equipment?.Primary;
                if (primaryEquipment == null) return;
                var compLightsaberBlade = compCache.GetCachedComp(primaryEquipment);
                if (compLightsaberBlade?.IsAnimatingNow == true)
                {
                    compLightsaberBlade.ResetToZero();
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(Thing), "Graphic", MethodType.Getter)]
        public static class Thing_DefaultGraphic_Patch
        {
            public static bool Prefix(ref Graphic __result, ref Thing __instance)
            {
                var graphicComp = compCacheGraphic.GetCachedComp(__instance);
                if (__instance == null || graphicComp != null) return true;
                var thingEq = __instance;
                var lightsaberComp = compCache.GetCachedComp(__instance);
                if (lightsaberComp != null && lightsaberComp.selectedhiltgraphic != null)
                {
                    var hiltDef = lightsaberComp.selectedhiltgraphic.hiltgraphic.Graphic;
                    if (hiltDef != null)
                    {
                        __result = hiltDef.GetColoredVersion(
                            hiltDef.Shader,
                            lightsaberComp.hiltColorOneOverrideColor,
                            lightsaberComp.hiltColorTwoOverrideColor
                        );
                        __instance.Notify_ColorChanged();
                        return false;
                    }
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(Thing), "get_UIIconOverride")]
        public static class Patch_UIIconOverride
        {
            static bool Prefix(ref Texture __result, ref Thing __instance)
            {
                var graphicComp = compCacheGraphic.GetCachedComp(__instance);
                if (__instance == null || graphicComp != null)
                {
                    return true;
                }
                var thingEq = __instance;
                var lightsabercomp = compCache.GetCachedComp(thingEq);
                if (lightsabercomp != null && lightsabercomp.selectedhiltgraphic != null)
                {
                    var hiltDef = lightsabercomp.selectedhiltgraphic.hiltgraphic.Graphic.MatSingle;
                    __result = hiltDef.mainTexture;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
        public static class Pawn_DraftedPatch
        {
            public static void Postfix(Pawn_DraftController __instance, bool value)
            {
                if (value) return;
                var pawn = __instance?.pawn;
                if (pawn == null || pawn.equipment == null)
                {
                    return;
                }
                var primaryEquipment = pawn.equipment.Primary;
                if (primaryEquipment == null)
                {
                    return;
                }
                var lightsabercomp = compCache.GetCachedComp(primaryEquipment);
                if (lightsabercomp != null)
                {
                    lightsabercomp.ResetToZero();
                }
                else return;
            }
        }

        [HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
        public static class Pawn_EquipmentTracker_GetGizmos_Patch
        {
            [HarmonyPostfix]
            public static void GetGizmosPostfix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result)
            {
                var lightsaberComp = compCache.GetCachedComp(__instance.Primary);
                if (lightsaberComp != null && __instance.pawn.Faction == Faction.OfPlayer)
                {
                    if (lightsaberComp.Props.colorable)
                    {
                        __result = __result?.Concat(lightsaberComp.EquippedGizmos()) ?? lightsaberComp.EquippedGizmos();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Projectile), "ImpactSomething")]
        public static class Patch_Projectile_ImpactSomething
        {
            public static bool Prefix(Projectile __instance)
            {
                if (__instance.usedTarget.Thing is Pawn pawn)
                {
                    if (pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance) is Hediff_LightsaberDeflection hediff)
                    {
                        if (hediff.ShouldDeflectProjectile(__instance) && __instance.Launcher != pawn)
                        {
                            hediff.DeflectProjectile(__instance);
                            return false;
                        }
                    }
                }
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

                    var ext = __instance.culture.GetModExtension<DefCultureExtension>();

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
            public static void Postfix(Mineable __instance, Map map)
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
                            itemColorable.SetColor(colorCrystalComp.parent.DrawColor);
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
                        var comp = compCache.GetCachedComp(product);
                        if (comp != null)
                        {
                            comp.SetLightsaberBlade1Color(crystalColor);
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

        [HarmonyPatch(typeof(Thing), "Notify_ColorChanged")]
        public static class Patch_ThingWithComps_Notify_ColorChanged
        {
            [HarmonyPostfix]
            public static void Postfix(ThingWithComps __instance)
            {
                // Check if the ThingWithComps has CompGlowerOptions
                var glowerComp = __instance.GetComp<CompGlower_Options>();
                if (glowerComp != null)
                {
                    // Update the glow color based on the parent’s DrawColor
                    ColorInt newGlowColor = new ColorInt(__instance.DrawColor);
                    glowerComp.UpdateGlowerColor(newGlowColor);
                }
            }
        }
    }
}


