using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using static HarmonyLib.Code;
using TheForce_Psycast.NightSisterMagick;

namespace TheForce_Psycast
{
    internal class AbilityExtensionForce : AbilityExtension_AbilityMod
    {
        public List<HediffDef> Attunement;
        public float severity = -1f;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);

            foreach (GlobalTargetInfo target in targets)
            {
                if (ability.pawn != null && Attunement != null)
                {
                    foreach (var def in Attunement)
                    {
                        var hediff = HediffMaker.MakeHediff(def, ability.pawn);
                        if (severity >= 0f)
                        {
                            hediff.Severity = severity;
                        }
                        ability.pawn.health.AddHediff(hediff);
                    }
                }
            }
        }
    }


    public class AbilityExtension_IchorCost : AbilityExtension_AbilityMod
    {
        public float IchorCost;
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            var Ichor = ability.pawn.genes.GetFirstGeneOfType<Force_GeneMagick>();
            Ichor.Resource.Value -= IchorCost;
        }

        public override string GetDescription(Ability ability)
        {
            return (("AbilityIchorCost".Translate() + ": ") + Mathf.RoundToInt(IchorCost * 100f)).ToString().Colorize(Color.green);
        }
    }

    public class AbilityExtension_PsycastHediffCheck : AbilityExtension_Hediff
    {
        public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
            if (target.Thing is not Pawn victim || victim.GetStatValue(StatDefOf.PsychicSensitivity) <= 0)
                return false;

            float casterSensitivity = ability.pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            float targetSensitivity = victim.GetStatValue(StatDefOf.PsychicSensitivity);

            if (targetSensitivity > casterSensitivity)
            {
                if (throwMessages)
                {
                    Messages.Message("Force_MindControlFailed".Translate(target.Pawn.Name), target.Thing, MessageTypeDefOf.NegativeEvent, true);
                }
                return false;
            }

            return base.ValidateTarget(target, ability, throwMessages);
        }
    }

    internal class AbilityExtensionHediffCheck : AbilityExtension_AbilityMod
    {
        public HediffDef HediffToCheck;  // The Hediff to check
        public float requiredSeverity = 0.5f;  // Required severity of the Hediff to cast the ability
        public float severityToSubtract = 0.1f;  // Amount of severity to subtract after casting the ability

        public override bool IsEnabledForPawn(Ability ability, out string reason)
        {
            if (!base.IsEnabledForPawn(ability, out reason))
                return false;

            if (ability.pawn != null && HediffToCheck != null)
            {
                Hediff hediff = ability.pawn.health.hediffSet.GetFirstHediffOfDef(HediffToCheck);
                if (hediff != null && hediff.Severity >= requiredSeverity)
                {
                    reason = null;
                    return true;
                }
                reason = "Force_HediffCheck".Translate(HediffToCheck.label, requiredSeverity.ToString("0.00"), (hediff != null ? hediff.Severity.ToString("0.00") : "0.00"));
                return false;
            }

            reason = "Cannot cast ability: Hediff or Pawn is null.";
            return false;
        }

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            if (ability.pawn != null && HediffToCheck != null)
            {
                // Find the Hediff on the pawn
                Hediff hediff = ability.pawn.health.hediffSet.GetFirstHediffOfDef(HediffToCheck);

                if (hediff != null && hediff.Severity >= requiredSeverity)
                {
                    // Subtract the specified amount of severity
                    hediff.Severity -= severityToSubtract;

                    // Ensure the severity does not go below 0
                    if (hediff.Severity < 0)
                    {
                        hediff.Severity = 0;
                    }

                    // Proceed with casting the ability
                    base.Cast(targets, ability);
                }
            }
        }
    }
}
    
