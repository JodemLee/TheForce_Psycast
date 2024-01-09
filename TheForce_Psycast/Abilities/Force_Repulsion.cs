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
    public class Ability_ForceRepulsion : VFECore.Abilities.Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            // Calculate the explosive force
            int explosiveForce = Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 7));

            foreach (GlobalTargetInfo target in targets)
            {

                // Calculate the offset between caster and target
                IntVec3 offset = target.Cell - this.pawn.Position;

                // Normalize the offset
                IntVec3 normalizedOffset = NormalizeIntVec3(offset);

                // Apply explosive force to the target
                if (target.Thing is Pawn)
                {
                    AbilityPawnFlyer flyer = (AbilityPawnFlyer)PawnFlyer.MakeFlyer(VFE_DefOf_Abilities.VFEA_AbilityFlyer, target.Thing as Pawn, target.Cell + (normalizedOffset * explosiveForce), null, null);
                    flyer.ability = this; ValidateTargetTile(pawn);
                    flyer.target = (target.Cell + (normalizedOffset * explosiveForce)).ToVector3();
                    GenSpawn.Spawn(flyer, this.pawn.Position, this.pawn.Map);
                }
                else
                {
                    target.Thing.Position = target.Cell + (normalizedOffset * explosiveForce);
                }
            }

            base.Cast(targets);
        }

        public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 2));

        private IntVec3 NormalizeIntVec3(IntVec3 vector)
        {
            float magnitude = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

            if (magnitude > 0)
            {
                float scaleFactor = 1f / magnitude;
                return new IntVec3(Mathf.RoundToInt(vector.x * scaleFactor), Mathf.RoundToInt(vector.y * scaleFactor), Mathf.RoundToInt(vector.z * scaleFactor));
            }

            return vector;
        }
    }
}