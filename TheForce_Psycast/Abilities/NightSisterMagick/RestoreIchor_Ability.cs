using RimWorld;
using RimWorld.Planet;
using TheForce_Psycast.NightSisterMagick;
using Verse;
using Ability = VFECore.Abilities.Ability;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    public class Ability_RestoreIchorFromPlants : Ability
    {
        public float radius = 1f; // Default value


        public override void Init()
        {
            base.Init();
            var nightsisterGene = pawn.genes.GetGene(ForceDefOf.Force_NightSisterMagick);
            if (nightsisterGene is null)
            {
                nightsisterGene = GeneMaker.MakeGene(ForceDefOf.Force_NightSisterMagick, this.pawn);
                pawn.genes.AddGene(nightsisterGene.def, true);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref radius, "radius", 1f); // Save and load the radius value
        }

        public override void Cast(GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            Pawn caster = CasterPawn;
            Map map = caster.Map;

            // Get the radius from the ability definition
            AbilityDef def = this.def; // Explicit cast to AbilityDef
            if (def != null)
            {
                radius = def.radius;
            }

            // Get all plants within the radius
            IntVec3 casterPosition = caster.Position;
            var plants = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
                .FindAll(plant => plant.Position.InHorDistOf(casterPosition, radius));

            // Calculate the amount of ichor to restore based on the number of plants
            float ichorRestoreAmount = CalculateIchorRestoreAmount(plants.Count);

            // Spawn mote from each affected plant
            foreach (var plant in plants)
            {
                // Create mote at the position of the plant
                MoteMaker.MakeStaticMote(plant.Position.ToVector3Shifted(), map, ForceDefOf.Mote_NightsisterMagickIchor, 1f);
            }

            // Adjust the ichor for the caster pawn
            Force_GeneMagick.OffsetIchor(caster, ichorRestoreAmount);
        }

        private float CalculateIchorRestoreAmount(int plantCount)
        {
            // Define your formula for calculating the ichor restore amount based on plant count
            // This is just a placeholder, replace it with your actual formula
            return plantCount * 0.1f;
        }
    }
}


