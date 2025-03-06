using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Verse.Sound;

namespace TheForce_Psycast.Abilities.Darkside
{
    internal class Force_Crush : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            int damageAmount = (int)GetPowerForPawn();
            if (targets[0].Thing is Pawn targetPawn)
            {
                ApplyInternalDamage(targetPawn, damageAmount);
            }
        }

        private void ApplyInternalDamage(Pawn targetPawn, int damageAmount)
        {
            // Get all internal body parts (excluding the outermost layer)
            var internalParts = targetPawn.health.hediffSet.GetNotMissingParts()
                .Where(part => part.depth == BodyPartDepth.Inside)
                .ToList();

            if (internalParts.Count == 0) return;
            foreach (var part in internalParts)
            {
                var damageInfo = new DamageInfo(DamageDefOf.Crush,damageAmount, 0, -1, this.pawn, part);
                targetPawn.TakeDamage(damageInfo);
            }
        }
    }
}

