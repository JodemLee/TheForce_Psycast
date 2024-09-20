
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities.Sith_Sorcery
{
    internal class SithRitual_Summon : VFECore.Abilities.Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var map = targets[0].Map;
            var entryCell = FindRandomEntryCell(map);

            // Get the ModExtension containing the animal list
            var modExtension = this.def.GetModExtension<ModExtension_AnimalList>();
            if (modExtension == null || modExtension.animalKinds.NullOrEmpty())
            {
                Log.Error("No animal kinds defined in ModExtension_AnimalList");
                return;
            }

            // Summon random animals from the list
            foreach (var animalKind in modExtension.animalKinds)
            {
                var animal = PawnGenerator.GeneratePawn(animalKind, Faction.OfPlayer);
                GenSpawn.Spawn(animal, entryCell, map);
                animal.mindState.mentalStateHandler.TryStartMentalState(VPE_DefOf.VPE_ManhunterTerritorial);
                animal.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(25000, 35000);
            }

            Find.LetterStack.ReceiveLetter("VPE.PackSummon".Translate(), "VPE.PackSummon.Desc".Translate(pawn.NameShortColored), LetterDefOf.PositiveEvent,
                new TargetInfo(entryCell, map));
        }

        private IntVec3 FindRandomEntryCell(Map map)
        {
            if (!RCellFinder.TryFindRandomPawnEntryCell(out var entryCell, map, CellFinder.EdgeRoadChance_Animal))
                entryCell = CellFinder.RandomEdgeCell(map);
            return entryCell;
        }
    }

    public class ModExtension_AnimalList : DefModExtension
    {
        // A list of PawnKindDefs to define the animals
        public List<PawnKindDef> animalKinds;
    }
}