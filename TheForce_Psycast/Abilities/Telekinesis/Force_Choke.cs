using RimWorld;
using RimWorld.Planet;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    internal class Force_Choke : VFECore.Abilities.Ability
    {
        public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 1) * 4);
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            foreach (var target in targets)
            {
                Pawn targetPawn = target.Thing as Pawn;

                if (targetPawn != null)
                {
                    // Check if the target pawn has a neck
                    var neckParts = targetPawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(part => part.def.tags.Contains(BodyPartTagDefOf.BreathingPathway)).ToList();

                    if (neckParts != null && neckParts.Count > 0)
                    {
                        // Deal damage to the target's neck
                        float damageAmount = GetPowerForPawn();
                        damageAmount = pawn.GetStatValue(StatDefOf.PsychicSensitivity) * damageAmount;

                        // Iterate over each neck part and apply damage
                        foreach (var neckPart in neckParts)
                        {
                            // Create a damage info object
                            DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, damageAmount, 0, -1, null, neckPart);

                            // Apply damage to the target pawn
                            targetPawn.TakeDamage(damageInfo);
                        }
                    }
                }
            }
        }
    }
}
