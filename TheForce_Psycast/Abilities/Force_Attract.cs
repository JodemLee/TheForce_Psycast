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

namespace TheForce_Psycast
{
    public class Ability_ForceAttract : VFECore.Abilities.Ability
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
                pullPosition.x = Mathf.Clamp(pullPosition.x, this.pawn.Position.x - 1, this.pawn.Position.x + 1);
                pullPosition.y = Mathf.Clamp(pullPosition.y, this.pawn.Position.y - 1, this.pawn.Position.y + 1);
                pullPosition.z = Mathf.Clamp(pullPosition.z, this.pawn.Position.z - 1, this.pawn.Position.z + 1);

                if (target.Thing is Pawn)
                {
                    AbilityPawnFlyer flyer = (AbilityPawnFlyer)PawnFlyer.MakeFlyer(VFE_DefOf_Abilities.VFEA_AbilityFlyer, target.Thing as Pawn, pullPosition, null, null);
                    flyer.ability = this; ValidateTargetTile(pawn);
                    flyer.target = pullPosition.ToVector3();
                    GenSpawn.Spawn(flyer, pullPosition, this.pawn.Map);
                }
                else
                {
                    target.Thing.Position = pullPosition;
                }

                base.Cast(targets);
            }
        }


        public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 2));

        // Custom method to normalize IntVec3
        private IntVec3 NormalizeIntVec3(IntVec3 vector)
        {
            float magnitude = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

            if (magnitude > 0)
            {
                float scaleFactor = 1f / magnitude;
                return new IntVec3(Mathf.RoundToInt(vector.x * scaleFactor), Mathf.RoundToInt(vector.y * scaleFactor), Mathf.RoundToInt(vector.z * scaleFactor));
            }

            return vector; // Avoid division by zero
        }
    }
}