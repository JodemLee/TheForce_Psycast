/*using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Abilities.Sith_Sorcery
{
    internal class CompObelisk_Sith : CompInteractable
    {
        public new CompProperties_ObeliskSith Props => (CompProperties_ObeliskSith)props;

        // Override the CompTick method to check for pawns in a radial around the object
        public override void CompTick()
        {
            base.CompTick(); // Call the base class method to retain existing functionality

            // Check if there are any pawns within a certain radius of the object
            bool pawnsInRange = CheckForPawnsInRange();

            // If there are pawns in range, apply the hediff
            if (pawnsInRange)
            {
                ApplyHediff();
            }
        }

        // Method to check for pawns within a certain radius of the object
        private bool CheckForPawnsInRange()
        {
            Map map = parent.Map;
            IntVec3 position = parent.Position;

            // Define the radius to check
            int radius = 5; // Adjust this value as needed

            // Iterate over all cells in the radial area
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, radius, true))
            {
                // Check if there's a pawn at the current cell
                if (cell.InBounds(map) && cell.GetFirstPawn(map) != null)
                {
                    return true; // Found a pawn within range
                }
            }

            return false; // No pawns found within range
        }

        // Method to apply the hediff to pawns found within range
        private void ApplyHediff()
        {
            // Get all pawns within the same map
            Map map = parent.Map;
            IntVec3 position = parent.Position;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, 5, true))
            {
                if (cell.InBounds(map))
                {
                    Pawn pawn = cell.GetFirstPawn(map);
                    if (pawn != null)
                    {
                        // Apply the hediff to the pawn
                        HediffDef hediffDef = HediffDefOf.Hypothermia; // Change this to the desired hediff
                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        pawn.health.AddHediff(hediff);
                    }
                }
            }
        }

        public class CompProperties_ObeliskSith : CompProperties_Interactable
        {
            [MustTranslate]
            public string messageActivating;

            public CompProperties_ObeliskSith()
            {
                compClass = typeof(CompProperties_ObeliskSith);
            }
        }

    }
}

*/