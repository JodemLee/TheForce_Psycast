using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore;
using UnityEngine;

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
                    var neckPart = targetPawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == ForceDefOf.Neck);

                    if (neckPart != null)
                    {
                        // Deal damage to the target's neck
                        float damageAmount = 5f; // You can adjust the damage amount as needed
                        damageAmount =  pawn.GetStatValue(StatDefOf.PsychicSensitivity)*damageAmount;

                        // Create a damage info object
                        DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, damageAmount, 0, -1, null, neckPart);

                        // Apply damage to the target pawn
                        targetPawn.TakeDamage(damageInfo);

                        // Ensure the neck's hit points don't go below 1
                        if (neckPart.def.hitPoints <= 0)
                        {
                            neckPart.def.hitPoints = 1;
                        }
                    }
                }
            }
        }
    }
}