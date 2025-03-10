﻿using RimWorld;
using RimWorld.Planet;
using TheForce_Psycast.Abilities;
using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    public class Ability_ForceAttract : Ability_WriteCombatLog
    {
        // Set your desired maximum pull distance
        private float maxPullDistance = 10f;
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            // Calculate the explosive force
            int explosiveForce = Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 7));

            foreach (GlobalTargetInfo target in targets)
            {
                // Calculate the offset between caster and target
                IntVec3 offset = this.pawn.Position - targets[0].Cell;

                // Calculate the normalized direction vector
                Vector3 direction = offset.ToVector3().normalized;

                // Calculate the pull position based on the normalized direction and capped distance
                IntVec3 pullPosition = this.pawn.Position + new IntVec3(Mathf.RoundToInt(direction.x * maxPullDistance), Mathf.RoundToInt(direction.y * maxPullDistance), Mathf.RoundToInt(direction.z * maxPullDistance));

                // Ensure the pull position does not go beyond the caster
                pullPosition.x = Mathf.Clamp(pullPosition.x, this.pawn.Position.x , this.pawn.Position.x );
                pullPosition.z = Mathf.Clamp(pullPosition.z, this.pawn.Position.z , this.pawn.Position.z );

                if (target.Thing is Pawn)
                {
                    var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawnAttract, target.Thing as Pawn, pullPosition, null, null);
                    ValidateTargetTile(pawn);
                    GenSpawn.Spawn(flyer, pullPosition, this.pawn.Map);
                }
                else
                {
                   return;
                }

                base.Cast(targets);
            }
        }
    }
}