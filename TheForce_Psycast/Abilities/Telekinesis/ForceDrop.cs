using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
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

        private void SpawnRandomChunk(IntVec3 pos, Map map)
        {
            // Choose a random ThingDef from the list
            ThingDef randomItemDef = itemDefs.RandomElement();

            // Spawn the selected item
            if (randomItemDef != null)
            {
                SkyfallerMaker.SpawnSkyfaller(randomItemDef, ThingDefOf.ShipChunk, pos, map);
            }
            else
            {
                Log.Error("Random item definition is null.");
            }
        }
    }
}

