using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using TheForce_Psycast.Lightsabers;
using Verse;

namespace TheForce_Psycast.Harmony_Patches
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Darkside_Patch
    {
        public static void Prefix(Pawn __instance, DamageInfo? dinfo)
        {
            if (Force_ModSettings.IncreaseDarksideOnKill)
            {
                if (dinfo.HasValue && dinfo.Value.Instigator is Pawn attacker)
                {
                    if (__instance.RaceProps.Humanlike)
                    {
                        if (!attacker.HostileTo(__instance.Faction))
                        {
                            DarksideUtility.AdjustHediffSeverity(attacker, ForceDefOf.Force_Darkside, 0.1f);
                        }
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(MemoryThoughtHandler))]
    [HarmonyPatch("TryGainMemory")]
    [HarmonyPatch(new Type[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class DarksideMemory_Patch
    {
        public static HashSet<string> DarksideThoughts = new HashSet<string>
    {
        "KnowGuestExecuted",
        "KnowColonistExecuted",
        "KnowPrisonerDiedInnocent",
        "KnowColonistDied",
        "PawnWithBadOpinionDied",
        "PawnWithBadOpinionLost",
        "KnowPrisonerSold",
        "KnowGuestOrganHarvested",
        "KnowColonistOrganHarvested",
        "KilledHumanlikeBloodlust",
        "ColonistBanished",
        "ColonistBanishedToDie",
        "PrisonerBanishedToDie",
        "KilledMyFriend",
        "KilledMyRival",
        "KilledMyLover",
        "KilledMyFiance",
        "KilledMySpouse",
        "KilledMyFather",
        "KilledMyMother",
        "KilledMySon",
        "KilledMyDaughter",
        "KilledMyBrother",
        "KilledMySister",
        "KilledMyKin",
        "KilledMyBondedAnimal",
        "SoldPrisoner",
        "SoldMyLovedOne",
        "ExecutedPrisoner",
        "KilledColonist",
        "KilledColonyAnimal",
        "OtherTravelerDied"
    };
        public static void PostFix(Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            if (Force_ModSettings.IncreaseDarksideOnKill)
            {
                if (DarksideThoughts.Contains(newThought.def.defName))
                {
                    DarksideUtility.AdjustHediffSeverity(__instance.pawn, ForceDefOf.Force_Darkside, 0.1f);
                }
            }

        }
    }

    [HarmonyPatch(typeof(Faction), "Notify_MemberTookDamage")]
    public static class Notify_MemberTookDamage_Patch
    {
        public static void Postfix(Pawn member, DamageInfo dinfo)
        {
            if (!Force_ModSettings.IncreaseDarksideOnKill || dinfo.Instigator is not Pawn attacker || member.HostileTo(attacker))
            {
                return;
            }

            DarksideUtility.AdjustHediffSeverity(attacker, ForceDefOf.Force_Darkside, 0.01f);
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_Slight), "RandomSelectionWeight")]
    public static class InteractionWorker_Slight_RandomSelectionWeight_Patch
    {
        public static void Postfix(Pawn initiator)
        {
            var severityIncrease = 0.01f;
            DarksideUtility.AdjustHediffSeverity(initiator, ForceDefOf.Force_Darkside, severityIncrease);
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_Insult), "RandomSelectionWeight")]
    public static class InteractionWorker_Insult_RandomSelectionWeight_Patch
    {
        public static void Postfix(Pawn initiator)
        {
            var severityIncrease = 0.01f;
            DarksideUtility.AdjustHediffSeverity(initiator, ForceDefOf.Force_Darkside, severityIncrease);
        }
    }

    [HarmonyPatch(typeof(TendUtility))]
    [HarmonyPatch("DoTend")]
    public static class TendUtility_DoTend_Patch
    {
        public static void Postfix(Pawn doctor, Pawn patient, Medicine medicine)
        {
            if (doctor != null)
            {
                float quality = TendUtility.CalculateBaseTendQuality(doctor, patient, medicine?.def);
                float severityIncrease = 0.01f * quality; // Adjust the base increase factor as needed

                // Increase the hediff on the doctor pawn
                DarksideUtility.AdjustHediffSeverity(doctor, ForceDefOf.Force_Lightside, severityIncrease);
            }
        }
    }

    [HarmonyPatch(typeof(IdeoUtility))]
    [HarmonyPatch("Notify_QuestCleanedUp")]
    public static class QuestManager_Notify_QuestCleanedUp_Patch
    {
        public static void Postfix(Quest quest, QuestState state)
        {
            if (quest != null && quest.charity && ModsConfig.IdeologyActive)
            {


                if (state == QuestState.EndedSuccess)
                {
                    List<Pawn> colonists = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists;
                    foreach (Pawn colonist in colonists)
                    {
                        var severityIncrease = 0.01f;
                        DarksideUtility.AdjustHediffSeverity(colonist, ForceDefOf.Force_Lightside, severityIncrease);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PeaceTalks))]
        [HarmonyPatch("Outcome_Success")]
        public static class PeaceTalks_Outcome_Success_Patch
        {
            // Postfix patch for Outcome_Success
            public static void Postfix(Caravan caravan)
            {
                ModifyHediffOnOutcome(caravan, 0.3f);
            }

            // Method to modify hediff
            private static void ModifyHediffOnOutcome(Caravan caravan, float severityIncrease)
            {
                foreach (Pawn pawn in caravan.PawnsListForReading)
                {
                    DarksideUtility.AdjustHediffSeverity(pawn, ForceDefOf.Force_Lightside, severityIncrease);
                }
            }
        }
    }



    [HarmonyPatch(typeof(PeaceTalks))]
    [HarmonyPatch("Outcome_Triumph")]
    public static class PeaceTalks_Outcome_Triumph_Patch
    {
        // Postfix patch for Outcome_Triumph
        public static void Postfix(Caravan caravan)
        {
            ModifyHediffOnOutcome(caravan, 0.8f); // Example: Increase hediff severity by 1.0
        }

        private static void ModifyHediffOnOutcome(Caravan caravan, float severityIncrease)
        {
            foreach (Pawn pawn in caravan.PawnsListForReading)
            {
                DarksideUtility.AdjustHediffSeverity(pawn, ForceDefOf.Force_Lightside, severityIncrease);
            }
        }
    }

    [HarmonyPatch(typeof(MechanitorUtility), "ShouldBeMechanitor")]
    public static class ShouldBeMechanitor_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(ForceDefOf.Force_MechuLinkImplant))
            {
                __result = true;
            }
        }
    }
}







