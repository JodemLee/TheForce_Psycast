using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace TheForce_Psycast
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmonyPatch = new("Psycast_ForceThe");
            harmonyPatch.PatchAll();
        }

        public static Harmony harmonyPatch;

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
    public static class GenRecipe_MakeRecipeProducts_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef)
        {
            if (recipeDef == ForceDefOf.Make_StoneBlocksAny && Rand.Chance(0.1f))
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
