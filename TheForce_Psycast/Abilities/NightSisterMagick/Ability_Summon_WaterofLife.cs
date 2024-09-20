using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class Ability_Summon_WaterofLife : Ability
    {
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

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Thing potion = ThingMaker.MakeThing(ForceDefOf.Force_WaterofLife);
            IntVec3 cell = this.pawn.Position + GenRadial.RadialPattern[Rand.RangeInclusive(2, GenRadial.NumCellsInRadius(4.9f))];
            GenSpawn.Spawn(potion, cell, this.pawn.Map);
            MoteMaker.MakeStaticMote(cell, this.pawn.Map, ForceDefOf.Mote_NightsisterMagickIchor, 1f);
        }
    }
}
