using RimWorld.Planet;
using RimWorld;
using Verse;
using Ability = VFECore.Abilities.Ability;
using TheForce_Psycast.NightSisterMagick;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class Summon_Object : Ability
    {
        float drainAmount = .01f;

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

            float finalDrain = 0f;

            foreach (GlobalTargetInfo target in targets)
            {
                if (target.HasThing && target.Thing != null)
                {
                    ThingDef copyDef = target.Thing.def;

                    if (copyDef.MadeFromStuff && target.Thing.Stuff != null)
                    {
                        Thing copy = ThingMaker.MakeThing(copyDef, target.Thing.Stuff);
                        GenPlace.TryPlaceThing(copy, target.Thing.Position, target.Thing.Map, ThingPlaceMode.Near);

                        // Spawn mote at the position of the copy
                        MoteMaker.MakeStaticMote(copy.Position, copy.Map, ForceDefOf.Mote_NightsisterMagickIchor, 1f);
                        finalDrain -= copy.MarketValue * drainAmount;
                    }
                    else
                    {
                        Thing copy = ThingMaker.MakeThing(copyDef);
                        GenPlace.TryPlaceThing(copy, target.Thing.Position, target.Thing.Map, ThingPlaceMode.Near);

                        // Spawn mote at the position of the copy
                        MoteMaker.MakeStaticMote(copy.Position, copy.Map, ForceDefOf.Mote_NightsisterMagickIchor, 1f);
                        finalDrain -= copy.MarketValue * drainAmount;
                    }
                }
            }

            Force_GeneMagick.OffsetIchor(this.pawn, finalDrain);
        }
    }
}


