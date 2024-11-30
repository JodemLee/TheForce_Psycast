using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
                LightsaberCombatUtility.DestroyLimb(CasterPawn, target.Pawn, manipulationTarget);
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

        private BodyPartRecord FindManipulationTarget(Pawn target)
        {
            var manipulationSources = target.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(part => part.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment)).ToList();
            return manipulationSources.Any() ? manipulationSources.RandomElement() : null;
        }
    }
}

