using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Hediffs
{
    public class HediffWithComps_Lightside : HediffWithComps
    {
        // Define your threshold severity level
        private const float ThresholdSeverity = 0.5f;

        public override void Tick()
        {
            base.Tick();

            // Check if severity reaches the threshold
            if (Severity >= ThresholdSeverity)
            {
                // De-age the pawn until it reaches age 18
                DeagePawn();
            }
        }

        private void DeagePawn()
        {
            // Assuming you have access to the pawn instance
            if (pawn != null)
            {
                // Calculate the target age (18 in this case)
                int targetAge = 18;

                // Calculate the age difference between the current age and the target age
                int ageDifference = Math.Max(0, pawn.ageTracker.AgeBiologicalYears - targetAge);

                // De-age the pawn by the age difference
                pawn.ageTracker.AgeBiologicalTicks -= ageDifference * GenDate.TicksPerYear;

                // You might want to inform the player or log the event
                Log.Message("Pawn de-aged due to severity of HediffWithComps_Lightside.");
            }
        }
    }
}
