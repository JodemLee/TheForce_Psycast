using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast.Abilities;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_AbilityBase : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            foreach (var targetInfo in targets)
            {
                var localTarget = (LocalTargetInfo)targetInfo;
                NotifyMeleeAttackOn(localTarget);

                if (LightsaberCombatUtility.CanParry(localTarget.Pawn, CasterPawn))
                {
                    IntVec3? overlapPoint = GetRandomCellBetween(pawn.Position, localTarget.Pawn.Position);
                    if (overlapPoint.HasValue)
                    {
                        LightsaberCombatUtility.TriggerWeaponRotationOnParry(pawn, localTarget.Pawn);

                        DamageInfo deflectInfo = new DamageInfo(DamageDefOf.Blunt, 0, 0, -1, pawn);
                        localTarget.Pawn.Drawer.Notify_DamageDeflected(deflectInfo);

                        Effecter effecter = new Effecter(ForceDefOf.Force_LClashOne);
                        effecter.Trigger(new TargetInfo(overlapPoint.Value, pawn.Map), TargetInfo.Invalid);
                        effecter.Cleanup();
                       
                    }
                    continue;
                }

                AttackTarget(localTarget);
            }
        }

        protected IntVec3? GetRandomCellBetween(IntVec3 casterPos, IntVec3 targetPos)
        {
            IEnumerable<IntVec3> lineCells = GenSight.PointsOnLineOfSight(casterPos, targetPos)
                .Where(cell => cell.InBounds(pawn.Map) && cell.Walkable(pawn.Map)).ToList();

            return lineCells.Any() ? lineCells.RandomElement() : null;
        }

        protected void NotifyMeleeAttackOn(LocalTargetInfo target)
        {
            if (target.HasThing && target.Thing.Position != pawn.Position)
            {
                pawn.Drawer.Notify_MeleeAttackOn(target.Thing);
            }
        }

        public virtual void AttackTarget(LocalTargetInfo target)
        {
            if (target.Pawn != null)
            {
                pawn.meleeVerbs.TryMeleeAttack(target.Pawn, pawn.equipment.PrimaryEq.PrimaryVerb, true);
            }
        }

        
    }
}
