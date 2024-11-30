using RimWorld;
using System.Linq;
using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_Ability_SaiCha : Lightsaber_AbilityBase
    {
        public override void AttackTarget(LocalTargetInfo target)
        {
            if (target.Pawn != null)
            {
                var manipulationTarget = FindManipulationTarget(target.Pawn);
                if (manipulationTarget != null)
                {
                    LightsaberCombatUtility.DestroyLimb(CasterPawn, target.Pawn, manipulationTarget);
                }
                else
                {
                    Messages.Message("No limbs are left to manipulate.", MessageTypeDefOf.RejectInput);
                }
            }
            else
            {
                Messages.Message("Invalid target.", MessageTypeDefOf.RejectInput);
            }
        }

        private BodyPartRecord FindManipulationTarget(Pawn target)
        {
            var manipulationSources = target.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(part => part.def.tags.Contains(BodyPartTagDefOf.BreathingPathway)).ToList();

            if (manipulationSources.Any())
            {
                return manipulationSources.RandomElement();
            }
            else
            {
                return null;
            }
        }
    }
}
