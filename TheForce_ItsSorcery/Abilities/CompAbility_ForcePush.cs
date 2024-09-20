using RimWorld;
using TheForce_ItsSorcery.Abilities;
using Verse;

namespace TheForce_ItsSorcery.Abilities
{
    internal class CompAbilityEffect_ForcePush : CompAbilityEffect
    {
        public new CompProperties_ForcePush Props => (CompProperties_ForcePush)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn == null)
            {
                Log.Warning("ForcePush: Target is not a pawn.");
                return;
            }

            // Get the stat value for scaling
            float statValue = Props.scalingStat != null ? pawn.GetStatValue(Props.scalingStat) : 1f;

            // If the stat value is below the threshold, do not apply the effect
            if (statValue <= Props.effectivenessThreshold)
            {
                Log.Warning("ForcePush: Stat value below effectiveness threshold.");
                return;
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
            IntVec3 launchDirection = pawn.Position - parent.pawn.Position;
            IntVec3 destination = pawn.Position + launchDirection * (int)knockback;

            // Check if the destination is valid
            if (!PositionUtils.CheckValidPosition(destination, parent.pawn.Map))
            {
                Log.Warning($"ForcePush: Destination {destination} is not valid.");
                destination = PositionUtils.FindValidPosition(pawn.Position, launchDirection, parent.pawn.Map);
            }

            PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer, pawn, destination, null, null);
            if (pawnFlyer != null)
            {
                GenSpawn.Spawn(pawnFlyer, destination, parent.pawn.Map);
                Log.Message($"ForcePush: PawnFlyer spawned at {destination}.");
            }
            else
            {
                Log.Warning("ForcePush: Failed to create PawnFlyer.");
            }
        }
    }


    public class CompProperties_ForcePush : CompProperties_AbilityEffect
    {
        public int baseKnockback = 5;
        public StatDef scalingStat; // The stat used for scaling
        public float scalingFactor = 1f;
        public bool scaleInversely = true; // Controls reverse scaling
        public float effectivenessThreshold = 0.2f; // Minimum stat value for the ability to have an effect

        public CompProperties_ForcePush()
        {
            compClass = typeof(CompAbilityEffect_ForcePush);
        }
    }
}
