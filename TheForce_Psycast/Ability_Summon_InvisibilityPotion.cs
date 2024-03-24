using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class Ability_Summon_InvisibilityPotion : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Thing potion = ThingMaker.MakeThing(ForceDefOf.Force_InvisibilityPotion);
            IntVec3 cell = this.pawn.Position + GenRadial.RadialPattern[Rand.RangeInclusive(2, GenRadial.NumCellsInRadius(4.9f))];
            GenSpawn.Spawn(potion, cell, this.pawn.Map);
            MoteMaker.MakeStaticMote(cell, this.pawn.Map, ForceDefOf.Mote_NightsisterMagickIchor, 1f);
        }
    }
}
