using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
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
                            var hediff = attacker.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside);
                            if (hediff != null)
                            {
                                hediff.Severity += 0.01f; // Increase the severity by 0.1
                            }
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
                    Hediff hediff = __instance.pawn.health?.hediffSet?.GetFirstHediffOfDef(ForceDefOf.Force_Darkside);
                    if (hediff != null)
                    {
                        // Adjust the severity increment as per your needs
                        hediff.Severity += 1f;
                    }
                }
            }

        }
    }

    [HarmonyPatch(typeof(Faction), "Notify_MemberTookDamage")]
    public static class Notify_MemberTookDamage_Patch
    {
        public static void Postfix(Pawn member, DamageInfo dinfo)
        {
            if (Force_ModSettings.IncreaseDarksideOnKill)
            {
                if (dinfo.Instigator is Pawn attacker)
                {
                    // Check if the IncreaseDarksideOnKill setting is enabled
                    bool increaseDarksideOnKill = Force_ModSettings.IncreaseDarksideOnKill;

                    if (increaseDarksideOnKill)
                    {
                        // Check if the attacker is not hostile to the member
                        if (!member.HostileTo(attacker))
                        {
                            // Get the Force_Darkside hediff of the attacker
                            var hediff = attacker.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside);
                            if (hediff != null)
                            {
                                // Increase the severity of the hediff
                                hediff.Severity += 0.01f; // Increase the severity by 0.01
                            }
                        }
                    }
                }
            }

        }
    }

    [HarmonyPatch(typeof(InteractionWorker_Slight), "RandomSelectionWeight")]
    public static class InteractionWorker_Slight_RandomSelectionWeight_Patch
    {
        public static void Postfix(Pawn initiator)
        {
            Hediff hediff = initiator.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside);
            if (hediff != null)
            {
                hediff.Severity += 0.01f; // Increase severity by 0.01
            }
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_Insult), "RandomSelectionWeight")]
    public static class InteractionWorker_Insult_RandomSelectionWeight_Patch
    {
        public static void Postfix(Pawn initiator)
        {
            Hediff hediff = initiator.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside);
            if (hediff != null)
            {
                hediff.Severity += 0.01f; // Increase severity by 0.01
            }
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
                Hediff hediff = doctor.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Lightside);
                if (hediff != null)
                {
                    hediff.Severity += severityIncrease;
                }
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
                        Hediff hediff = colonist.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Lightside);
                        if (hediff != null)
                        {
                            hediff.Severity += 0.3f; // Adjust the severity increase as needed
                        }
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
                ModifyHediffOnOutcome(caravan, 0.3f); // Example: Increase hediff severity by 0.5
            }

            // Method to modify hediff
            private static void ModifyHediffOnOutcome(Caravan caravan, float severityIncrease)
            {
                foreach (Pawn pawn in caravan.PawnsListForReading)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Lightside); // Replace with your actual hediff def
                    if (hediff != null)
                    {
                        hediff.Severity += severityIncrease;
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

            // Method to modify hediff (same as above, can reuse the existing method)
            private static void ModifyHediffOnOutcome(Caravan caravan, float severityIncrease)
            {
                foreach (Pawn pawn in caravan.PawnsListForReading)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Lightside);
                    if (hediff != null)
                    {
                        hediff.Severity += severityIncrease;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(InteractionWorker_KindWords), "RandomSelectionWeight")]
        //public static class InteractionWorker_KindWords_RandomSelectionWeight_Patch
        //{
        //    public static void Postfix(Pawn initiator)
        //    {
        //        Hediff hediff = initiator.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Lightside);
        //        if (hediff != null)
        //        {
        //            hediff.Severity += 0.01f; // Increase severity by 0.01
        //        }
        //    }
        //}
    }
}







