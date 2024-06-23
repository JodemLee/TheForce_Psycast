using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_Ability_ChoMai : Lightsaber_AbilityBase
    {
        public override void AttackTarget(LocalTargetInfo target)
        {
            var manipulationTarget = FindManipulationTarget(target.Pawn);
            if (manipulationTarget != null)
            {
                // Calculate damage and destroy the limb
                DestroyLimb(target.Pawn, manipulationTarget);

                // Drop the weapon if the target has one
                if (target.Pawn.equipment != null && target.Pawn.equipment.Primary != null)
                {
                    ThingWithComps weapon = target.Pawn.equipment.Primary;
                    target.Pawn.equipment.TryDropEquipment(weapon, out ThingWithComps droppedWeapon, target.Pawn.Position, true);
                }
            }
            else
            {
                Messages.Message("No manipulable limbs are left.", MessageTypeDefOf.RejectInput);
            }
        }

        private void DestroyLimb(Pawn target, BodyPartRecord limb)
        {
            int damageAmount = CalculateDamageToDestroyLimb(target, limb);
            Log.Message($"Damage needed to destroy limb: {damageAmount}");

            ThingDef weaponDef = this.pawn.equipment?.Primary?.def;

            DamageDef cutDamage = DamageDefOf.Cut;
            var damageInfo = new DamageInfo(cutDamage, damageAmount, 50, -1, Caster, limb, weaponDef);

            // Ensure that at least 1 damage is dealt to guarantee limb destruction
            target.TakeDamage(damageInfo);
        }

        private int CalculateDamageToDestroyLimb(Pawn target, BodyPartRecord limb)
        {
            // Return a very high damage value to ensure limb destruction
            return 50; // You can adjust this value as needed
        }

        private BodyPartRecord FindManipulationTarget(Pawn target)
        {
            // Filter body parts that can be manipulated
            var manipulationSources = target.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(part => part.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment)).ToList();

            // Return a random manipulable limb
            return manipulationSources.Any() ? manipulationSources.RandomElement() : null;
        }
    }
}

