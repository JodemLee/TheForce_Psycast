using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using VanillaPsycastsExpanded;
using Verse;

namespace TheForce_Psycast
{
    public class HediffComp_Heal : HediffComp
    {
        private HediffCompProperties_Heal Props => (HediffCompProperties_Heal)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Find.TickManager.TicksGame % Props.healIntervalTicks == 0)
            {
                bool healedOnce = false;

                // Heal chronic conditions
                if (Props.healChronicConditions)
                {
                    var chronicHediff = Pawn.health.hediffSet.hediffs
                        .Where(hd => hd.IsPermanent() || hd.def.chronic)
                        .ToList();

                    if (chronicHediff.TryRandomElement(out var result))
                    {
                        HealthUtility.Cure(result);
                        healedOnce = true;
                    }
                }

                // Heal injuries
                if (Props.healInjuries && !healedOnce)
                {
                    var injuredHediffs = Pawn.health.hediffSet.hediffs
                        .OfType<Hediff_Injury>()
                        .ToList();

                    if (injuredHediffs.Any())
                    {
                        injuredHediffs.RandomElement().Heal(Props.injuryHealAmount);
                        healedOnce = true;
                    }
                }

                // Regrow missing parts
                if (Props.regrowMissingParts && !healedOnce)
                {
                    var nonMissingParts = Pawn.health.hediffSet.GetNotMissingParts().ToList();
                    var missingParts = Pawn.def.race.body.AllParts
                        .Where(x => Pawn.health.hediffSet.PartIsMissing(x)
                            && nonMissingParts.Contains(x.parent)
                            && !Pawn.health.hediffSet.AncestorHasDirectlyAddedParts(x))
                        .ToList();

                    if (missingParts.Any())
                    {
                        var missingPart = missingParts.RandomElement();
                        var currentMissingHediffs = Pawn.health.hediffSet.hediffs
                            .OfType<Hediff_MissingPart>()
                            .ToList();

                        Pawn.health.RestorePart(missingPart);
                        var regeneratingHediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("Force_Regen"), Pawn, missingPart);
                        regeneratingHediff.Severity = missingPart.def.GetMaxHealth(Pawn) - 1; // Start with max health - 1
                        Pawn.health.AddHediff(regeneratingHediff);

                        healedOnce = true;
                    }
                }

                // Show healing effect if something was healed
                if (healedOnce)
                {
                    FleckMaker.ThrowMetaIcon(Pawn.Position, Pawn.Map, FleckDefOf.HealingCross);
                }

                // Remove the hediff if there are no more wounds to heal
                if (!HasAnyWounds())
                {
                    Pawn.health.RemoveHediff(parent);
                }
            }
        }

        // Helper method to check if the pawn has any wounds, chronic conditions, or missing parts
        private bool HasAnyWounds()
        {
            if (Props.healInjuries && Pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().Any())
            {
                return true;
            }
            if (Props.healChronicConditions && Pawn.health.hediffSet.hediffs.Any(hd => hd.IsPermanent() || hd.def.chronic))
            {
                return true;
            }

            // Check for missing parts
            if (Props.regrowMissingParts)
            {
                var nonMissingParts = Pawn.health.hediffSet.GetNotMissingParts().ToList();
                var missingParts = Pawn.def.race.body.AllParts
                    .Where(x => Pawn.health.hediffSet.PartIsMissing(x)
                        && nonMissingParts.Contains(x.parent)
                        && !Pawn.health.hediffSet.AncestorHasDirectlyAddedParts(x))
                    .ToList();

                if (missingParts.Any())
                {
                    return true;
                }
            }

            return false;
        }

        public override string CompTipStringExtra
        {
            get
            {
                int totalTicksRemaining = CalculateTotalHealingTime();
                if (totalTicksRemaining > 0)
                {
                    return "Force.TimeRemaining".Translate(totalTicksRemaining.ToStringTicksToPeriod());
                }
                return null;
            }
        }

        private int CalculateTotalHealingTime()
        {
            int totalTicks = 0;
            if (Props.healInjuries)
            {
                var injuredHediffs = Pawn.health.hediffSet.hediffs
                    .OfType<Hediff_Injury>()
                    .ToList();

                foreach (var injury in injuredHediffs)
                {
                    // Calculate the number of healing ticks required for this injury
                    int ticksForInjury = (int)(injury.Severity / Props.injuryHealAmount) * Props.healIntervalTicks;
                    totalTicks += ticksForInjury;
                }
            }

            // Calculate healing time for chronic conditions
            if (Props.healChronicConditions)
            {
                var chronicHediffs = Pawn.health.hediffSet.hediffs
                    .Where(hd => hd.IsPermanent() || hd.def.chronic)
                    .ToList();

                // Assume each chronic condition takes a fixed amount of time to heal
                totalTicks += chronicHediffs.Count * Props.healIntervalTicks;
            }

            // Calculate healing time for missing parts
            if (Props.regrowMissingParts)
            {
                var nonMissingParts = Pawn.health.hediffSet.GetNotMissingParts().ToList();
                var missingParts = Pawn.def.race.body.AllParts
                    .Where(x => Pawn.health.hediffSet.PartIsMissing(x)
                        && nonMissingParts.Contains(x.parent)
                        && !Pawn.health.hediffSet.AncestorHasDirectlyAddedParts(x))
                    .ToList();

                // Assume each missing part takes a fixed amount of time to regrow
                totalTicks += missingParts.Count * Props.healIntervalTicks;
            }

            return totalTicks;
        }
    }

    public class HediffCompProperties_Heal : HediffCompProperties
    {
        public int healIntervalTicks = GenDate.TicksPerHour;
        public float injuryHealAmount = 3f;
        public bool healInjuries = true;
        public bool healChronicConditions = true;
        public bool regrowMissingParts = true;

        public HediffCompProperties_Heal()
        {
            compClass = typeof(HediffComp_Heal);
        }
    }
}