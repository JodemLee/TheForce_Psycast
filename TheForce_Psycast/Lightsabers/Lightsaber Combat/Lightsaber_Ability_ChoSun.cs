﻿using RimWorld;
using System.Linq;
using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_Ability_ChoSun : Lightsaber_AbilityBase
    {
        public override void AttackTarget(LocalTargetInfo target)
        {
            var manipulationTarget = FindManipulationTarget(target.Pawn);
            if (manipulationTarget != null)
            {
                DestroyLimb(target.Pawn, manipulationTarget);
            }
            else
            {
                Messages.Message("No limbs are left to manipulate.", MessageTypeDefOf.RejectInput);
            }
        }

        private void DestroyLimb(Pawn target, BodyPartRecord limb)
        {
            int damageAmount = CalculateDamageToDestroyLimb(limb, target);

            ThingDef weaponDef = null;
            if (this.pawn.equipment?.Primary != null)
            {
                weaponDef = this.pawn.equipment.Primary.def;
            }

            DamageDef cutDamage = DamageDefOf.Cut;
            var damageInfo = new DamageInfo(cutDamage, damageAmount, 50, -1, Caster, limb, weaponDef);
            target.TakeDamage(damageInfo);
        }

        private int CalculateDamageToDestroyLimb(BodyPartRecord limb, Pawn target)
        {
            // Get the current health of the limb
            return 50; //
        }

        private BodyPartRecord FindManipulationTarget(Pawn target)
        {
            var manipulationSources = target.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(part => part.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbCore) || part.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment)).ToList();

            if (manipulationSources.Any())
            {
                // If there are manipulation sources, return a random one
                return manipulationSources.RandomElement();
            }
            else
            {
                // If there are no manipulation sources, return null
                return null;
            }
        }
    }
}
