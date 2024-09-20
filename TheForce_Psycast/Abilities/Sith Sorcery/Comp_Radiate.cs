using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheForce_Psycast.Abilities.Sith_Sorcery
{
    internal class Comp_Radiate : ThingComp
    {
        public CompProperties_Radiate Props => (CompProperties_Radiate)props;

        private float radius;

        // Define two hediffs: one for friendly pawns and one for hostile pawns
        private HediffDef friendlyHediff;
        private HediffDef hostileHediff;

        // Define the amount by which to increase or decrease stats for friendly and hostile pawns
        private float friendlyIncreaseAmount = 0.1f;
        private float hostileIncreaseAmount = 0.1f;

        // Define how frequently the stat enhancement should be applied (in ticks)
        private int applyFrequency = 300;
        private int tickCounter = 0; // Counter to keep track of ticks

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            CompProperties_Radiate compProps = (CompProperties_Radiate)props;

            // Initialize the hediffs, amounts, and frequency from the properties
            friendlyHediff = compProps.friendlyHediff;
            hostileHediff = compProps.hostileHediff;
            friendlyIncreaseAmount = compProps.friendlyIncreaseAmount;
            hostileIncreaseAmount = compProps.hostileIncreaseAmount;
            applyFrequency = compProps.applyFrequency;
            radius  = compProps.radius;
        }

        public override void CompTick()
        {
            base.CompTick();

            // Increase the tick counter
            tickCounter++;

            // Apply the stat enhancement only if the tick counter has reached the apply frequency
            if (tickCounter >= applyFrequency)
            {
                ApplyStatEnhancement();
                tickCounter = 0; // Reset the counter
            }
        }

        private void ApplyStatEnhancement()
        {
            Map map = parent.Map;
            if (map == null)
            {
                return;
            }

            // Get all pawns in the radius
            IEnumerable<Pawn> pawnsInRadius = map.mapPawns.AllPawnsSpawned
                .Where(p => p.Position.InHorDistOf(parent.Position, radius));

            foreach (Pawn pawn in pawnsInRadius)
            {
                // Check if the pawn is from the same faction
                if (pawn.Faction == parent.Faction)
                {
                    // Apply friendly hediff
                    ApplyHediff(pawn, friendlyHediff, friendlyIncreaseAmount);
                }
                else if (pawn.HostileTo(parent.Faction))
                {
                    // Apply hostile hediff
                    ApplyHediff(pawn, hostileHediff, hostileIncreaseAmount);
                }
            }
        }

        // Apply the hediff to the pawn, adding it if not already present
        private void ApplyHediff(Pawn pawn, HediffDef hediffDef, float amount)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                pawn.health.AddHediff(hediff);
                hediff.Severity = hediff.def.initialSeverity;
            }

            // Adjust severity based on the defined amount
            hediff.Severity += amount;
        }
    }

    // Define the properties for the custom ThingComp
    public class CompProperties_Radiate : CompProperties
    {
        // Two hediffs for friendly and hostile pawns
        public float radius = 10f;

        public HediffDef friendlyHediff;
        public HediffDef hostileHediff;

        // Amount to adjust stats for friendly and hostile pawns
        public float friendlyIncreaseAmount = 0.1f;
        public float hostileIncreaseAmount = 0.1f;

        // How frequently the stat enhancement should be applied (in ticks)
        public int applyFrequency = 300;

        public CompProperties_Radiate()
        {
            compClass = typeof(Comp_Radiate);
        }
    }
}
