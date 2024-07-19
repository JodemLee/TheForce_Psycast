using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using TheForce_Psycast;
using TheForce_Psycast.Abilities.Lightside;
using TheForce_Psycast.Hediffs;
using UnityEngine;
using Verse;

public static class ForceGhost_HarmonyPatch
{
    [StaticConstructorOnStartup]
    public static class ConsolidatedGhostsPatch
    {
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

        [HarmonyPatch(typeof(HealthCardUtility), "DrawMedOperationsTab")]
        public static class PatchDrawMedOperationsTab
        {
            public static bool Prefix(Pawn pawn)
            {
                if (ForceGhostUtility.IsForceGhost(pawn))
                {
                    return false;
                }
                return true;
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

        [HarmonyPatch(typeof(Thing), "Destroy")]
        public static class Thing_Destroy_Patch
        {
            public static void Postfix(Thing __instance)
            {
                // Create a list to hold the pawns that need to be processed
                List<Pawn> pawnsToProcess = new List<Pawn>();

                // Collect all pawns that need to be processed
                foreach (var map in Find.Maps)
                {
                    foreach (var pawn in map.mapPawns.AllPawns)
                    {
                        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside) as HediffWithComps_DarksideGhost;
                        var ghostHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Ghost);

                        if (hediff != null && ForceGhostUtility.IsForceGhost(pawn) && hediff.linkedObject == __instance)
                        {
                            pawnsToProcess.Add(pawn);
                        }
                    }
                }

                // Process the collected pawns outside of the iteration
                foreach (var pawn in pawnsToProcess)
                {
                    var ghostHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Ghost);
                    if (ghostHediff != null)
                    {
                        pawn.health.RemoveHediff(ghostHediff);
                    }
                    pawn.Kill(null);
                }
            }
        }


        [HarmonyPatch(typeof(TradeDeal), "TryExecute", new[] { typeof(bool) }, new[] { ArgumentType.Out })]
        public static class TradeDeal_TryExecute_Warning_Patch
        {
            public static bool Prefix(List<Tradeable> ___tradeables)
            {
                // Check if any tradeable is a linked object
                bool linkedObjectFound = ___tradeables.Any(tradeable => IsLinkedObject(tradeable.ThingDef));

                if (linkedObjectFound)
                {
                    // Display a warning to the player
                    Messages.Message("Warning: One or more items in this trade are linked objects. Selling them will result in their linked items being destroyed.", MessageTypeDefOf.RejectInput);

                    // Optionally, you can return false to cancel the trade if needed
                    // return false;
                }

                // Continue with the trade execution
                return true;
            }

            private static bool IsLinkedObject(ThingDef def)
            {
                foreach (var map in Find.Maps) // Iterate through all maps
                {
                    foreach (var pawn in map.mapPawns.AllPawns) // Get all pawns on each map
                    {
                        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside) as HediffWithComps_DarksideGhost;
                        if (hediff != null && hediff.linkedObject != null && hediff.linkedObject.def == def)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
