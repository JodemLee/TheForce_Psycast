using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TheForce_Psycast.Comps;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;
using Verse.AI;


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
                postfix: new HarmonyMethod(type, nameof(DrawEquipmentAimingPostFix)));
            harmonyPatch.PatchAll();

        }

        public static Harmony harmonyPatch;

        public static void DrawEquipmentAimingPostFix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            var compLightsaberBlade = eq?.TryGetComp<Comp_LightsaberBlade>();
            if (compLightsaberBlade?.Graphic == null)
                return;

            compLightsaberBlade.PostDraw(drawLoc, eq.Rotation, aimAngle);
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


        [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
        public class GenRecipe_MakeRecipeProducts_Patch
        {
            public float dropChance { get; set; }

            public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef)
            {
                float dropChance = Force_ModSettings.dropChance;
                if (recipeDef == ForceDefOf.Make_StoneBlocksAny && Rand.Chance(dropChance))
                {
                    ThingDef extraMaterialDef = ForceDefOf.Force_KyberCrystal;
                    Thing extraMaterial = ThingMaker.MakeThing(extraMaterialDef);
                    extraMaterial.stackCount = 1;
                    __result = __result.Append(extraMaterial);
                }
                if (recipeDef == ForceDefOf.Make_StoneBlocksGranite && Rand.Chance(dropChance))
                {
                    ThingDef extraMaterialDef = ForceDefOf.Force_KyberCrystal;
                    Thing extraMaterial = ThingMaker.MakeThing(extraMaterialDef);
                    extraMaterial.stackCount = 1;
                    __result = __result.Append(extraMaterial);
                }
                if (recipeDef == ForceDefOf.Make_StoneBlocksLimestone && Rand.Chance(dropChance))
                {
                    ThingDef extraMaterialDef = ForceDefOf.Force_KyberCrystal;
                    Thing extraMaterial = ThingMaker.MakeThing(extraMaterialDef);
                    extraMaterial.stackCount = 1;
                    __result = __result.Append(extraMaterial);
                }
                if (recipeDef == ForceDefOf.Make_StoneBlocksMarble && Rand.Chance(dropChance))
                {
                    ThingDef extraMaterialDef = ForceDefOf.Force_KyberCrystal;
                    Thing extraMaterial = ThingMaker.MakeThing(extraMaterialDef);
                    extraMaterial.stackCount = 1;
                    __result = __result.Append(extraMaterial);
                }
                if (recipeDef == ForceDefOf.Make_StoneBlocksSandstone && Rand.Chance(dropChance))
                {
                    ThingDef extraMaterialDef = ForceDefOf.Force_KyberCrystal;
                    Thing extraMaterial = ThingMaker.MakeThing(extraMaterialDef);
                    extraMaterial.stackCount = 1;
                    __result = __result.Append(extraMaterial);
                }
                if (recipeDef == ForceDefOf.Make_StoneBlocksSlate && Rand.Chance(dropChance))
                {
                    ThingDef extraMaterialDef = ForceDefOf.Force_KyberCrystal;
                    Thing extraMaterial = ThingMaker.MakeThing(extraMaterialDef);
                    extraMaterial.stackCount = 1;
                    __result = __result.Append(extraMaterial);
                }
                return __result;
            }
        }
    }
}