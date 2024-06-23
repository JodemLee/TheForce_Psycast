using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VanillaPsycastsExpanded;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast
{
    public class Craft_SithCrystal : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Thing crystal = ThingMaker.MakeThing(ForceDefOf.Force_Ancient_Sith_HealingCrystal);
            IntVec3 cell = this.pawn.Position + GenRadial.RadialPattern[Rand.RangeInclusive(2, GenRadial.NumCellsInRadius(4.9f))];
            GenSpawn.Spawn(crystal, cell, this.pawn.Map);
        }
    }
}