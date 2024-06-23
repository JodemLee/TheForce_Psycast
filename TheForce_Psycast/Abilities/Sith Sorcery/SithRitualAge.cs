using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using Verse.AI;

namespace TheForce_Psycast.Abilities
{

    public class Ability_SithRitual : Ability_TargetCorpse
    {

        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            toil.AddPreInitAction(delegate
            {
                foreach (var target in Comp.currentlyCastingTargets)
                {
                    if (target.HasThing && target.Thing.TryGetComp<CompRottable>() != null)
                    {
                        this.AddEffecterToMaintain(VPE_DefOf.VPE_Liferot.Spawn(target.Thing.Position, this.pawn.Map), target.Thing, toil.defaultDuration);
                    }
                }
            });
            toil.AddPreTickAction(delegate
            {
                foreach (var target in Comp.currentlyCastingTargets)
                {
                    if (target.HasThing && target.Thing.TryGetComp<CompRottable>() != null && target.Thing.IsHashIntervalTick(60))
                    {
                        FilthMaker.TryMakeFilth(target.Thing.Position, target.Thing.Map, ThingDefOf.Filth_CorpseBile, 1);
                        target.Thing.TryGetComp<CompRottable>().RotProgress += 60000;
                    }
                }
            });
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (!pawn.health.hediffSet.HasHediff(VPE_DefOf.VPE_BodiesConsumed))
            {
                pawn.health.AddHediff(VPE_DefOf.VPE_BodiesConsumed);
            }

            var consumerHediff = this.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_BodiesConsumed) as Hediff_BodiesConsumed;
            foreach (var target in targets)
            {
                MoteBetween mote = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_SoulOrbTransfer);
                mote.Attach(target.Thing, this.pawn);
                mote.exactPosition = target.Thing.DrawPos;
                GenSpawn.Spawn(mote, target.Thing.Position, this.pawn.Map);
                consumerHediff.consumedBodies++;
                target.Thing.Destroy();
            }
        }
    }
    internal class AbilityExtension_Age : AbilityExtension_AbilityMod
    {
        public float? casterYears = null;
        public float? targetYears = null;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            if (casterYears.HasValue) Age(ability.pawn, casterYears.Value);
            if (!targetYears.HasValue) return;
            foreach (var target in targets)
                if (target.Thing is Pawn pawn)
                    Age(pawn, targetYears.Value);
        }

        public override bool CanApplyOn(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
            if (!base.CanApplyOn(target, ability, throwMessages)) return false;
            if (!targetYears.HasValue) return true;
            if (target.Thing is not Pawn pawn) return false;
            if (!pawn.RaceProps.IsFlesh) return false;
            if (!pawn.RaceProps.Humanlike) return false;
            return true;
        }

        public static void Age(Pawn pawn, float years)
        {
            // Calculate the new biological age
            int newBiologicalAge = Mathf.FloorToInt((pawn.ageTracker.AgeBiologicalYears + years) * GenDate.TicksPerYear);

            // Ensure the new age is not below 18
            newBiologicalAge = Mathf.Max(newBiologicalAge, Mathf.FloorToInt(18 * GenDate.TicksPerYear));

            // Calculate the age delta
            int ageDelta = (int)(newBiologicalAge - pawn.ageTracker.AgeBiologicalTicks);

            // Apply the age delta
            pawn.ageTracker.AgeBiologicalTicks += ageDelta;

            if (years < 0)
            {
                var giverSets = pawn.def.race.hediffGiverSets;
                if (giverSets != null)
                {
                    var lifeFraction = pawn.ageTracker.AgeBiologicalYears / pawn.def.race.lifeExpectancy;
                    foreach (var giverSet in giverSets)
                        foreach (var giver in giverSet.hediffGivers)
                            if (giver is HediffGiver_Birthday giverBirthday)
                                if (giverBirthday.ageFractionChanceCurve.Evaluate(lifeFraction) <= 0f)
                                {
                                    Hediff hediff;
                                    while ((hediff = pawn.health.hediffSet.GetFirstHediffOfDef(giverBirthday.hediff)) != null)
                                        pawn.health.RemoveHediff(hediff);
                                }
                }
            }

            if (pawn.ageTracker.AgeBiologicalYears > pawn.def.race.lifeExpectancy * 1.1f &&
                (pawn.genes == null || pawn.genes.HediffGiversCanGive(VPE_DefOf.HeartAttack)))
            {
                var part = pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Heart);
                var hediff = HediffMaker.MakeHediff(VPE_DefOf.HeartAttack, pawn, part);
                hediff.Severity = 1.1f;
                pawn.health.AddHediff(hediff, part);
            }
        }
    }
}
