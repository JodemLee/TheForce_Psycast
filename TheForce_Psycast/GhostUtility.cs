using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace TheForce_Psycast
{
    public static class ForceGhostUtility
    {
        public static bool ThingIsGhost(Thing t)
        {
            return t is Pawn p && PawnIsGhost(p);
        }

        public static bool PawnIsGhost(Pawn p)
        {
            if (p != null && !p.Dead && p.health != null && p.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.ForceGhost) != null)
            {
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn_AgeTracker))]
    class Pawn_AgeTracker_BiologicalTicksPerTick_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("BiologicalTicksPerTick", MethodType.Getter)]
        public static bool BiologicalTicksPerTick_ForceGhostPatch(ref Pawn ___pawn, ref float __result)
        {
            if (ForceGhostUtility.PawnIsGhost(___pawn))
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Building_SubcoreScanner))]
    [HarmonyPatch("CanAcceptPawn")]
    public static class Building_SubcoreScanner_CanAcceptPawn_Patch
    {
        [HarmonyPostfix]
        public static void SubcoreScanner_CanAcceptPawn_ForceGhostPatch(Pawn selPawn, ref AcceptanceReport __result)
        {
            if (__result && selPawn != null)
            {
                if (ForceGhostUtility.PawnIsGhost(selPawn))
                {
                    __result = "ESCP_NecromanticForceGhosts_PawnIsGhost".Translate(selPawn.Name);
                }
            }
        }
    }

    [HarmonyPatch(typeof(CompAbilityEffect_BloodfeederBite))]
    [HarmonyPatch("Valid")]
    public static class CompAbilityEffect_BloodfeederBite_Valid_Patch
    {
        [HarmonyPostfix]
        public static void BloodfeederBite_Valid_ForceGhostPatch(LocalTargetInfo target, ref bool __result)
        {
            Pawn pawn = target.Pawn;
            if (__result && pawn != null)
            {
                if (ForceGhostUtility.PawnIsGhost(pawn))
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_GetHemogen))]
    [HarmonyPatch("CanFeedOnPrisoner")]
    public static class JobGiver_GetHemogen_CanFeedOnPrisoner_Patch
    {
        [HarmonyPostfix]
        public static void CanFeedOnPrisoner_ForceGhostPatch(Pawn prisoner, ref AcceptanceReport __result)
        {
            if (__result)
            {
                if ( ForceGhostUtility.PawnIsGhost(prisoner))
                {
                    __result = "ESCP_NecromanticForceGhosts_PawnIsGhost".Translate(prisoner.Name);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Recipe_ExtractHemogen))]
    [HarmonyPatch("CompletableEver")]
    public static class Recipe_ExtractHemogen_CompletableEver_Patch
    {
        [HarmonyPostfix]
        public static void CompletableEver_ForceGhostPatch(Pawn surgeryTarget, ref bool __result)
        {
            if (__result)
            {
                if (ForceGhostUtility.PawnIsGhost(surgeryTarget))
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Recipe_ExtractHemogen))]
    [HarmonyPatch("CheckForWarnings")]
    public static class Recipe_ExtractHemogen_CheckForWarnings_Patch
    {
        [HarmonyPostfix]
        public static void CheckForWarnings_ForceGhostPatch(Pawn medPawn)
        {
            if (ForceGhostUtility.PawnIsGhost(medPawn))
            {
                Messages.Message("MessageCannotStartHemogenExtraction".Translate(medPawn.Named("PAWN")), medPawn, MessageTypeDefOf.NeutralEvent, false);
            }
        }
    }

    [HarmonyPatch(typeof(HediffGiver_Bleeding))]
    [HarmonyPatch("OnIntervalPassed")]
    class HediffGiver_Bleeding_OnIntervalPassed_Patch
    {
        [HarmonyPrefix]
        public static bool OnIntervalPassed_ForceGhostPatch(Pawn pawn)
        {
            return !(ForceGhostUtility.PawnIsGhost(pawn));
        }
    }

    [HarmonyPatch(typeof(HediffGiver_Hypothermia))]
    [HarmonyPatch("OnIntervalPassed")]
    class HediffGiver_Hypothermia_OnIntervalPassed_Patch
    {
        [HarmonyPrefix]
        public static bool OnIntervalPassed_ForceGhostPatch(Pawn pawn)
        {
            return !(ForceGhostUtility.PawnIsGhost(pawn));
        }
    }

    [HarmonyPatch(typeof(HediffGiver_Heat))]
    [HarmonyPatch("OnIntervalPassed")]
    class HediffGiver_Heat_OnIntervalPassed_Patch
    {
        [HarmonyPrefix]
        public static bool OnIntervalPassed_ForceGhostPatch(Pawn pawn)
        {
            return !(ForceGhostUtility.PawnIsGhost(pawn));
        }
    }

    [HarmonyPatch(typeof(Pawn_IdeoTracker))]
    class Pawn_IdeoTracker_CertaintyChangePerDay_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CertaintyChangePerDay", MethodType.Getter)]
        public static bool CertaintyChangePerDay_ForceGhostPatch(ref Pawn ___pawn, ref float __result)
        {
            if (ForceGhostUtility.PawnIsGhost(___pawn))
            {
                __result = 1f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_IdeoTracker))]
    [HarmonyPatch("SetIdeo")]
    public static class Pawn_IdeoTracker_SetIdeo_Patch
    {
        [HarmonyPrefix]
        public static bool SetIdeo_ForceGhostPatch(ref Pawn ___pawn)
        {
            if (ForceGhostUtility.PawnIsGhost(___pawn))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(InspirationWorker))]
    [HarmonyPatch("InspirationCanOccur")]
    public static class InspirationWorker_InspirationCanOccur_Patch
    {
        [HarmonyPrefix]
        public static bool InspirationCanOccur_ForceGhostPatch(Pawn pawn, ref bool __result)
        {
            if (ForceGhostUtility.PawnIsGhost(pawn))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(InteractionUtility))]
    [HarmonyPatch("CanInitiateInteraction")]
    public static class InteractionUtility_CanInitiateInteraction_Patch
    {
        [HarmonyPostfix]
        public static void CanInitiateInteraction_ForceGhostPatch(ref bool __result, Pawn pawn, InteractionDef interactionDef = null)
        {
            if (interactionDef != null && __result && ForceGhostUtility.PawnIsGhost(pawn))
            {
                if (interactionDef == RimWorld.InteractionDefOf.RomanceAttempt)
                {
                    __result = false;
                }

                if (interactionDef.modContentPack?.PackageId == "jpt.speakup")
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnUtility))]
    [HarmonyPatch("IsInteractionBlocked")]
    public static class PawnUtility_IsInteractionBlocked_Patch
    {
        [HarmonyPostfix]
        public static void IsInteractionBlocked_ForceGhostPatch(Pawn pawn, InteractionDef interaction, ref bool __result)
        {
            if (!__result && ForceGhostUtility.PawnIsGhost(pawn))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(MentalStateWorker))]
    [HarmonyPatch("StateCanOccur")]
    public static class MentalStateWorker_StateCanOccur_Patch
    {
        [HarmonyPrefix]
        public static bool StateCanOccur_ForceGhostPatch(Pawn pawn, ref bool __result)
        {
            if (ForceGhostUtility.PawnIsGhost(pawn))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Need))]
    class Need_IsFrozen_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("IsFrozen", MethodType.Getter)]
        public static bool IsFrozen_ForceGhostPatch(ref Need __instance, ref Pawn ___pawn, ref bool __result)
        {
            if (ForceGhostUtility.PawnIsGhost(___pawn))
            {
                if (__instance.def.defName != "TM_Mana" && __instance.def.defName != "TM_Stamina")
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Need))]
    class Need_ShowOnNeedList_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ShowOnNeedList", MethodType.Getter)]
        public static bool ShowOnNeedList_ForceGhostPatch(ref Need __instance, ref Pawn ___pawn, ref bool __result)
        {
            if (ForceGhostUtility.PawnIsGhost(___pawn))
            {
                if (__instance.def.defName != "TM_Mana" && __instance.def.defName != "TM_Stamina")
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Need))]
    class Need_CurLevel_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CurLevel", MethodType.Getter)]
        public static bool CurLevel_ForceGhostPatch(ref Need __instance, ref Pawn ___pawn, ref float __result)
        {
            if ( ForceGhostUtility.PawnIsGhost(___pawn))
            {
                if (__instance.def.defName != "TM_Mana" && __instance.def.defName != "TM_Stamina")
                {
                    __result = __instance.MaxLevel;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Need))]
    class Need_CurInstantLevel_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CurInstantLevel", MethodType.Getter)]
        public static bool CurInstantLevel_ForceGhostPatch(ref Need __instance, ref Pawn ___pawn, ref float __result)
        {
            if ( ForceGhostUtility.PawnIsGhost(___pawn))
            {
                if (__instance.def.defName != "TM_Mana" && __instance.def.defName != "TM_Stamina")
                {
                    __result = __instance.MaxLevel;
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Because Chemical just has to be special
    /// </summary>

    [HarmonyPatch(typeof(Need_Chemical_Any))]
    class NeedChemicalAny_Disabled_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Disabled", MethodType.Getter)]
        public static void Disabled_ForceGhostPatch(ref Pawn ___pawn, ref bool __result)
        {
            if (!__result && ForceGhostUtility.PawnIsGhost(___pawn))
            {
                __result = true;
            }
        }
    }

    /// <summary>
    /// Because Indoors just has to be special
    /// </summary>
    [HarmonyPatch(typeof(Need_Indoors))]
    class NeedIndoors_Disabled_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Disabled", MethodType.Getter)]
        public static void Disabled_ForceGhostPatch(ref Pawn ___pawn, ref bool __result)
        {
            if (!__result && ForceGhostUtility.PawnIsGhost(___pawn))
            {
                __result = true;
            }
        }
    }

    /// <summary>
    /// Because Outdoors just has to be special
    /// </summary>
    [HarmonyPatch(typeof(Need_Outdoors))]
    class NeedOutdoors_Disabled_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Disabled", MethodType.Getter)]
        public static void Disabled_ForceGhostPatch(ref Pawn ___pawn, ref bool __result)
        {
            if (!__result && ForceGhostUtility.PawnIsGhost(___pawn))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("Learn")]
    public static class Interval_Learn_Patch
    {
        [HarmonyPrefix]
        public static bool SkillRecord_Learn_ForceGhostPatch(ref Pawn ___pawn)
        {
            if (ForceGhostUtility.PawnIsGhost(___pawn))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("Interval")]
    public static class Interval_Interval_Patch
    {
        [HarmonyPrefix]
        public static bool SkillRecord_Interval_ForceGhostPatch(ref Pawn ___pawn)
        {
            if (ForceGhostUtility.PawnIsGhost(___pawn))
            {
                return false;
            }
            return true;
        }
    }

    //Works for trade caravans (at player map) and trade ships
    [HarmonyPatch(typeof(TradeUtility))]
    [HarmonyPatch("AllSellableColonyPawns")]
    public static class TradeUtility_AllSellableColonyPawns_Patch
    {
        [HarmonyPostfix]
        public static void AllSellableColonyPawns_ForceGhostPatch(ref IEnumerable<Pawn> __result)
        {
            List<Pawn> toRemove = new List<Pawn> { };
            foreach (Pawn p in __result)
            {
                if (ForceGhostUtility.PawnIsGhost(p))
                {
                    toRemove.Add(p);
                }
            }

            if (!toRemove.NullOrEmpty())
            {
                List<Pawn> editedList = __result.ToList();
                foreach (Pawn p in toRemove)
                {

                    editedList.Remove(p);
                }
                __result = editedList;
                toRemove.Clear();
            }

        }
    }

    //For trading directly with settlements
    [HarmonyPatch(typeof(Settlement_TraderTracker))]
    [HarmonyPatch("ColonyThingsWillingToBuy")]
    public static class Settlement_TraderTracker_ColonyThingsWillingToBuy_Patch
    {
        [HarmonyPostfix]
        public static void ColonyThingsWillingToBuy_ForceGhostPatch(ref IEnumerable<Thing> __result)
        {
            List<Thing> toRemove = new List<Thing> { };
            foreach (Thing t in __result)
            {
                if (ForceGhostUtility.ThingIsGhost(t))
                {
                    toRemove.Add(t);
                }
            }

            if (!toRemove.NullOrEmpty())
            {
                List<Thing> editedList = __result.ToList();
                foreach (Thing t in toRemove)
                {
                    editedList.Remove(t);
                }
                __result = editedList;
                toRemove.Clear();
            }

        }
    }

    //Not sure what this one is for, but keeping it in in case it is necessary
    [HarmonyPatch(typeof(Caravan_TraderTracker))]
    [HarmonyPatch("ColonyThingsWillingToBuy")]
    public static class Caravan_TraderTracker_ColonyThingsWillingToBuy_Patch
    {
        [HarmonyPostfix]
        public static void ColonyThingsWillingToBuy_ForceGhostPatch(ref IEnumerable<Thing> __result)
        {
            List<Thing> toRemove = new List<Thing> { };
            foreach (Thing t in __result)
            {
                if (ForceGhostUtility.ThingIsGhost(t))
                {
                    toRemove.Add(t);
                }
            }

            if (!toRemove.NullOrEmpty())
            {
                List<Thing> editedList = __result.ToList();
                foreach (Thing t in toRemove)
                {
                    editedList.Remove(t);
                }
                __result = editedList;
                toRemove.Clear();
            }

        }
    }

}

