using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_ItsSorcery.Abilities
{
    internal class CompAbilityEffect_ForceRepulse : CompAbilityEffect
    {
        public new CompProperties_ForceRepulse Props => (CompProperties_ForceRepulse)props;

        private Pawn Pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Log.Message("ForceRepulse Apply method called.");

            List<Thing> thingsInRange = ThingsInRange();
            Log.Message($"Found {thingsInRange.Count} things in range.");


            foreach (Thing thing in thingsInRange)
            {
                if (thing is Pawn pawn && pawn != Pawn)
                {
                    // Get the stat value for scaling
                    float statValue = Props.scalingStat != null ? pawn.GetStatValue(Props.scalingStat) : 1f;

                    // If the stat value is below the threshold, do not apply the effect
                    if (statValue <= Props.effectivenessThreshold)
                    {
                        continue;
                    }

                    // Calculate knockback with an option for reverse scaling
                    float knockback = Props.baseKnockback;
                    if (Props.scaleInversely)
                    {
                        knockback *= Props.scalingFactor / statValue;
                    }
                    else
                    {
                        knockback *= Props.scalingFactor * statValue;
                    }

                    // Launch the pawn
                    IntVec3 launchDirection = pawn.Position - Pawn.Position;
                    IntVec3 destination = pawn.Position + launchDirection * (int)knockback;

                    // Check if the destination is valid
                    if (!PositionUtils.CheckValidPosition(destination, Pawn.Map))
                    {
                        Log.Warning($"ForceRepulse: Destination {destination} is not valid.");
                        destination = PositionUtils.FindValidPosition(pawn.Position, launchDirection, Pawn.Map);
                    }

                    PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer, pawn, destination, null, null);
                    if (pawnFlyer != null)
                    {
                        GenSpawn.Spawn(pawnFlyer, destination, Pawn.Map);
                        Log.Message($"ForceRepulse: PawnFlyer spawned at {destination}.");
                    }
                    else
                    {
                        Log.Warning("ForceRepulse: Failed to create PawnFlyer.");
                    }
                }
            }
        }

        private List<Thing> ThingsInRange()
        {
            // Get all things in the effect radius around the caster
            IEnumerable<Thing> thingsInRange = GenRadial.RadialDistinctThingsAround(Pawn.Position, Pawn.Map, parent.def.EffectRadius, useCenter: true);
            return new List<Thing>(thingsInRange);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction != null)
            {
                foreach (Thing thing in ThingsInRange())
                {
                    if (thing.Faction == Pawn.Faction)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(new List<IntVec3>(ThingsInRange().Select(t => t.Position)));
        }
    }

    public class CompProperties_ForceRepulse : CompProperties_AbilityEffect
    {
        public int baseKnockback = 5;
        public StatDef scalingStat; // The stat used for scaling
        public float scalingFactor = 1f;
        public bool scaleInversely = true; // Controls reverse scaling
        public float effectivenessThreshold = 0.2f; // Minimum stat value for the ability to have an effect

        public CompProperties_ForceRepulse()
        {
            compClass = typeof(CompAbilityEffect_ForceRepulse);
        }
    }
}
