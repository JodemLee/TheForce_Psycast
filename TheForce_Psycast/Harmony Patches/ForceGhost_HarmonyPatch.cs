using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheForce_Psycast;
using TheForce_Psycast.Hediffs;
using UnityEngine;
using Verse;

public static class ForceGhost_HarmonyPatch
{
    [StaticConstructorOnStartup]
    public static class ConsolidatedGhostsPatch
    {
        public static class ForceGhost_HarmonyPatch
        {
            private static readonly Dictionary<Pawn, Color?> ghostColorCache = new Dictionary<Pawn, Color?>();

            public static void InvalidateCache(Pawn pawn)
            {
                // Method to invalidate the cache for a specific pawn
                ghostColorCache.Remove(pawn);
            }

            [HarmonyPatch(typeof(Pawn_StoryTracker), "get_SkinColor")]
            public static class SkinColorPostfixPatch
            {
                [HarmonyPriority(Priority.Last)]
                public static void Postfix(Pawn ___pawn, ref Color __result)
                {
                    Color? ghostColor = ForceGhostUtility.GetGhostColor(___pawn);
                    if (ghostColor.HasValue)
                    {
                        __result = ghostColor.Value;
                    }
                }
            }

            [HarmonyPatch(typeof(Pawn_StoryTracker), "get_HairColor")]
            public static class HairColorPostfixPatch
            {
                [HarmonyPriority(Priority.Last)]
                public static void Postfix(Pawn ___pawn, ref Color __result)
                {
                    Color? ghostColor = ForceGhostUtility.GetGhostColor(___pawn);
                    if (ghostColor.HasValue)
                    {
                        __result = ghostColor.Value;
                    }
                }
            }

            [HarmonyPatch(typeof(Apparel), "DrawColor", MethodType.Getter)]
            public static class Apparel_DrawColor_Patch
            {
                public static void Postfix(Apparel __instance, ref Color __result)
                {
                    // Check if the apparel has a wearer
                    Pawn wearer = __instance.Wearer;
                    if (wearer != null)
                    {
                        Color? ghostColor = ForceGhostUtility.GetGhostColor(wearer);
                        if (ghostColor.HasValue)
                        {
                            __result = ghostColor.Value;
                        }
                    }
                }
            }
        }
    

        [HarmonyPatch(typeof(HealthCardUtility), "DrawMedOperationsTab")]
        public static class PatchDrawMedOperationsTab
        {
            public static bool Prefix(Pawn pawn)
            {
                return !ForceGhostUtility.IsForceGhost(pawn); // Directly return the result of the condition
            }
        }

        [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "TryGiveThoughts")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(DamageInfo?), typeof(PawnDiedOrDownedThoughtsKind) })]
        public static class TryGiveThoughts_Patch
        {
            public static bool Prefix(Pawn victim)
            {
                return !ForceGhostUtility.IsForceGhost(victim);
            }
        }

        [HarmonyPatch(typeof(CompAbilityEffect_BloodfeederBite), "Valid")]
        public static class CompAbilityEffect_BloodfeederBitePostfixPatch
        {
            public static void Postfix(ref bool __result, LocalTargetInfo target, bool throwMessages)
            {
                Pawn pawn = target.Pawn;
                if (pawn != null && ForceGhostUtility.IsForceGhost(pawn))
                {
                    if (throwMessages)
                    {
                        Messages.Message("MessageCannotFeedOnGhost".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    __result = false;
                }
            }
        }
        
        [HarmonyPatch(typeof(TradeDeal), "TryExecute", new[] { typeof(bool) }, new[] { ArgumentType.Out })]
        public static class TradeDeal_TryExecute_Warning_Patch
        {
            public static bool Prefix(List<Tradeable> ___tradeables)
            {
                // Check if any tradeable is a linked object
                bool LinkedObjectFound = ___tradeables.Any(tradeable => IsLinkedObject(tradeable.ThingDef));

                if (LinkedObjectFound)
                {
                    // Display a warning to the player
                    Messages.Message("Warning: One or more items in this trade are linked objects. Selling them will result in their linked items being destroyed.", MessageTypeDefOf.RejectInput);
                }

                // Continue with the trade execution
                return true;
            }

            private static bool IsLinkedObject(ThingDef def)
            {
                foreach (var map in Find.Maps)
                {
                    foreach (var pawn in map.mapPawns.AllPawns)
                    {
                        var ghostHediff = pawn.health.hediffSet.hediffs
                            .FirstOrDefault(h => h.TryGetComp<HediffComp_Ghost>() != null);

                        if (ghostHediff != null)
                        {
                            var ghostComp = ghostHediff.TryGetComp<HediffComp_Ghost>();
                            if (!ghostComp.Props.isLightSide && ghostComp.LinkedObject != null && ghostComp.LinkedObject.def == def)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Thing), "GetInspectString")]
        public static class Patch_Thing_GetInspectString
        {
            private static readonly Dictionary<Thing, string> cachedResults = new Dictionary<Thing, string>();
            private static int lastCachedTick = -1;

            public static void Postfix(Thing __instance, ref string __result)
            {
                if (__instance == null || Find.CurrentMap == null)
                {
                    return;
                }
                int currentTick = Find.TickManager.TicksGame;
                if (lastCachedTick == currentTick && cachedResults.TryGetValue(__instance, out var cachedResult))
                {
                    __result = cachedResult;
                    return;
                }
                if (lastCachedTick != currentTick)
                {
                    cachedResults.Clear();
                    lastCachedTick = currentTick;
                }
                StringBuilder stringBuilder = null;
                bool foundLinkedPawn = false;

                foreach (var pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
                {
                    // Check if the pawn has any hediff with the Ghost comp
                    var ghostHediff = pawn.health?.hediffSet?.hediffs
                        .FirstOrDefault(h => h.TryGetComp<HediffComp_Ghost>() != null);

                    if (ghostHediff != null)
                    {
                        var ghostComp = ghostHediff.TryGetComp<HediffComp_Ghost>();
                        if (!ghostComp.Props.isLightSide && ghostComp.LinkedObject == __instance)
                        {
                            if (!foundLinkedPawn)
                            {
                                stringBuilder = new StringBuilder(__result);
                                foundLinkedPawn = true;
                            }
                            if (stringBuilder.Length > 0 && !stringBuilder.ToString().EndsWith("\n"))
                            {
                                stringBuilder.AppendLine();
                            }
                            stringBuilder.Append("SithGhost".Translate() + pawn.LabelCap);
                        }
                    }
                }

                // If linked pawn was found, set the result and cache it
                if (foundLinkedPawn)
                {
                    __result = stringBuilder.ToString();
                    cachedResults[__instance] = __result;
                }
            }
        }
    }
}
