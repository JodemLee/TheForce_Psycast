using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
               postfix: new HarmonyMethod(type, nameof(DrawEquipmentAimingPostFix)));
            harmonyPatch.PatchAll();
        }

        public static Harmony harmonyPatch;

        public static void DrawEquipmentAimingPostFix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Melee Animation"))
            {
                // If the mod is loaded, return and do nothing
                return;
            }

            if (eq == null || eq.def == null || eq.def.graphicData == null)
            {
                Log.Error("DrawEquipmentAimingPostFix: eq or its properties are null");
                return;
            }

            // Calculate angle and flip based on aimAngle and equippedAngleOffset
            bool flip = false;
            float angle = aimAngle - 90f;

            if (aimAngle > 20f && aimAngle < 160f)
            {
                angle += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                flip = true;
                angle -= 180f + eq.def.equippedAngleOffset;
            }
            else
            {
                angle += eq.def.equippedAngleOffset;
            }

            angle %= 360f;

            // Offset drawLoc slightly below parent def's position for the blade
            float coreYOffset = -0.001f; // Adjust this value as needed
            Vector3 coreDrawLoc = drawLoc;
            coreDrawLoc.y += coreYOffset;

            float bladeYOffset = -0.002f; // Adjust this value as needed
            Vector3 bladeDrawLoc = drawLoc;
            bladeDrawLoc.y += bladeYOffset;

            // Scale for the blade and cores
            Vector3 scale = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);

            // Matrix for the blade
            Matrix4x4 bladeMatrix = Matrix4x4.TRS(bladeDrawLoc, Quaternion.AngleAxis(angle, Vector3.up), scale);

            // Matrix for the cores (same as blade for now, adjust if necessary)
            Matrix4x4 coreMatrix = Matrix4x4.TRS(coreDrawLoc, Quaternion.AngleAxis(angle, Vector3.up), scale);

            // Draw the blade
            Comp_LightsaberBlade bladeComp = eq.TryGetComp<Comp_LightsaberBlade>();
            if (bladeComp == null)
            {
                Log.Error("DrawEquipmentAimingPostFix: Comp_LightsaberBlade is null");
                return;
            }

            DrawMesh(bladeComp.Graphic, bladeMatrix, flip, bladeComp.Props.Altitude);

            // Draw the cores if they exist
            if (bladeComp.LightsaberCore1Graphic != null)
            {
                DrawMesh(bladeComp.LightsaberCore1Graphic, coreMatrix, flip, bladeComp.Props.Altitude);
            }
            if (bladeComp.LightsaberBlade2Graphic != null)
            {
                DrawMesh(bladeComp.LightsaberBlade2Graphic, bladeMatrix, flip, bladeComp.Props.Altitude);
            }
            if (bladeComp.LightsaberCore2Graphic != null)
            {
                DrawMesh(bladeComp.LightsaberCore2Graphic, coreMatrix, flip, bladeComp.Props.Altitude);
            }
        }


        public static void DrawMesh(Graphic graphic, Matrix4x4 matrix, bool flip, float altitude)
        {
            if (graphic != null)
            {
                Material material = graphic.MatSingle;
                Graphics.DrawMesh(flip ? MeshPool.plane10Flip : MeshPool.plane10, matrix, material, (int)altitude);
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