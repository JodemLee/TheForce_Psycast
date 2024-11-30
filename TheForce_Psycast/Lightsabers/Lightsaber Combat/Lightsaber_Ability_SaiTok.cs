using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_Ability_SaiTok : Lightsaber_AbilityBase
    {
        public override void AttackTarget(LocalTargetInfo target)
        {
            var manipulationTargets = FindManipulationTarget(target.Pawn);
            if (manipulationTargets != null && manipulationTargets.Any())
            {
                var limb = manipulationTargets.RandomElement();
                LightsaberCombatUtility.DestroyLimb(CasterPawn, target.Pawn, limb);
            }
            else
            {
                Messages.Message("No limbs are left to amputate.", MessageTypeDefOf.RejectInput);
            }
        }

        private List<BodyPartRecord> FindManipulationTarget(Pawn target)
        {
            var manipulationSources = target.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(part => part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore)).ToList();

            if (manipulationSources.Any())
            {
                return manipulationSources;
            }
            else
            {
                return null;
            }
        }
    }
}
