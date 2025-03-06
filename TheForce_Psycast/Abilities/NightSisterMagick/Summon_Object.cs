using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class Summon_Object : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (GlobalTargetInfo target in targets)
            {
                if (target.HasThing && target.Thing != null)
                {
                    ThingDef copyDef = target.Thing.def;

                    if (copyDef.MadeFromStuff && target.Thing.Stuff != null)
                    {
                        Thing copy = ThingMaker.MakeThing(copyDef, target.Thing.Stuff);
                        GenPlace.TryPlaceThing(copy, target.Thing.Position, target.Thing.Map, ThingPlaceMode.Near);
                    }
                    else
                    {
                        Thing copy = ThingMaker.MakeThing(copyDef);
                        GenPlace.TryPlaceThing(copy, target.Thing.Position, target.Thing.Map, ThingPlaceMode.Near);
                    }
                }
            }
        }
    }
}


