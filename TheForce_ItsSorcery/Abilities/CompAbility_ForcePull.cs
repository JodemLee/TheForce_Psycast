using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_ItsSorcery.Abilities
{
    internal class CompAbilityEffect_ForcePull : CompAbilityEffect
    {
        public new CompProperties_ForcePull Props => (CompProperties_ForcePull)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn == null)
            {
                return;
            }

            float statValue = Props.scalingStat != null ? pawn.GetStatValue(Props.scalingStat) : 1f;

            if (statValue <= Props.effectivenessThreshold)
            {
                return;
            }

            float pullDistance = Props.basePullDistance;
            if (Props.scaleInversely)
            {
                pullDistance *= Props.scalingFactor / statValue;
            }
            else
            {
                pullDistance *= Props.scalingFactor * statValue;
            }

            IntVec3 pullDirection = parent.pawn.Position - pawn.Position;
            float length = pullDirection.LengthHorizontal;
            if (length == 0f)
            {
                return;
            }

            pullDirection = new IntVec3(
                Mathf.RoundToInt(pullDirection.x / length),
                0,
                Mathf.RoundToInt(pullDirection.z / length)
            );

            IntVec3 potentialDestination = pawn.Position + pullDirection * (int)pullDistance;

            IntVec3 destination = pawn.Position;
            if (potentialDestination != pawn.Position)
            {
                float maxDistance = (parent.pawn.Position - pawn.Position).LengthHorizontal;
                if (pullDistance > maxDistance)
                {
                    pullDistance = maxDistance;
                }
                destination = pawn.Position + pullDirection * (int)pullDistance;
            }

            if (destination.InBounds(parent.pawn.Map) && !destination.Filled(parent.pawn.Map))
            {
                PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer, pawn, destination, null, null);
                GenSpawn.Spawn(pawnFlyer, destination, parent.pawn.Map);
            }
        }
    }

    public class CompProperties_ForcePull : CompProperties_AbilityEffect
    {
        public int basePullDistance = 5;
        public StatDef scalingStat; // The stat used for scaling
        public float scalingFactor = 1f;
        public bool scaleInversely = true; // Controls reverse scaling
        public float effectivenessThreshold = 0.2f; // Minimum stat value for the ability to have an effect

        public CompProperties_ForcePull()
        {
            compClass = typeof(CompAbilityEffect_ForcePull);
        }
    }
}
