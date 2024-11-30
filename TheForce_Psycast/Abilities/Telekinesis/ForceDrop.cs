using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Telekinesis
{
    internal class ForceDrop : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            foreach (var target in targets)
            {
                IntVec3 targetPos = target.Cell;

                if (targetPos.IsValid && targetPos.InBounds(target.Map))
                {
                    SpawnRandomChunk(targetPos, target.Map);
                }
                else
                {
                    Log.Error("Invalid target position for dropping ship chunk.");
                }
            }
        }

        private List<ThingDef> itemDefs = new List<ThingDef>
        {
        ThingDefOf.ShipChunkIncoming,
        ThingDefOf.ShuttleCrashing,
        ThingDefOf.CrashedShipPartIncoming,
         };

        private List<float> itemWeights = new List<float> { 1f, 3f, 1f }; // Weights for each ThingDef

        private void SpawnRandomChunk(IntVec3 pos, Map map)
        {
            // Ensure the list lengths match
            if (itemDefs.Count != itemWeights.Count)
            {
                Log.Error("itemDefs and itemWeights lists must have the same length.");
                return;
            }

            // Calculate total weight
            float totalWeight = itemWeights.Sum();

            // Generate a random value between 0 and total weight
            float randomValue = Rand.Range(0f, totalWeight);

            // Select the ThingDef based on the weighted randomness
            float cumulativeWeight = 0f;
            for (int i = 0; i < itemDefs.Count; i++)
            {
                cumulativeWeight += itemWeights[i];
                if (randomValue <= cumulativeWeight)
                {
                    ThingDef selectedDef = itemDefs[i];
                    SkyfallerMaker.SpawnSkyfaller(selectedDef, ThingDefOf.ShipChunk, pos, map);
                    return;
                }
            }

            // Fallback in case no item is selected (shouldn't happen)
            Log.Error("No item selected after applying weighted randomness.");
        }
    }
}

