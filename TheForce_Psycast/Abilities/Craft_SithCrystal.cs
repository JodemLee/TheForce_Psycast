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
        public float DarksideConnection => pawn.GetStatValue(ForceDefOf.Force_Darkside_Attunement);

        private static readonly AccessTools.FieldRef<Pawn_PsychicEntropyTracker, float> currentEntropy =
            AccessTools.FieldRefAccess<Pawn_PsychicEntropyTracker, float>("currentEntropy");


        public override bool IsEnabledForPawn(out string reason)
        {
            if (!base.IsEnabledForPawn(out reason)) return false;
            if (DarksideConnection > 1.5f) return true;
            reason = "Force.NotEnoughAttunement".Translate();
            return false;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            this.pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside).Severity -= 0.3f;
            Thing crystal = ThingMaker.MakeThing(ForceDefOf.Force_Ancient_Sith_HealingCrystal);
            IntVec3 cell = this.pawn.Position + GenRadial.RadialPattern[Rand.RangeInclusive(2, GenRadial.NumCellsInRadius(4.9f))];
            GenSpawn.Spawn(crystal, cell, this.pawn.Map);
        }

        public override string GetDescriptionForPawn() => base.GetDescriptionForPawn() + "\n" + "Force.MustHaveDarkAttuneAmount".Translate(150).Colorize(Color.red);
    }
}