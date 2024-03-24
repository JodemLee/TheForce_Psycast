using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaPsycastsExpanded.Skipmaster;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast
{
    public class Ability_ForcePull : VFECore.Abilities.Ability
    {
        // Set your desired maximum pull distance
        private float maxPullDistance = 10f;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            foreach (GlobalTargetInfo target in targets)
            {
                // Calculate the offset between caster and target
                IntVec3 offset = this.pawn.Position - targets[0].Cell;

                // Calculate the normalized direction vector
                Vector3 direction = offset.ToVector3().normalized;

                // Calculate the pull position based on the normalized direction and capped distance
                IntVec3 pullPosition = this.pawn.Position + new IntVec3(Mathf.RoundToInt(direction.x * maxPullDistance), Mathf.RoundToInt(direction.y * maxPullDistance), Mathf.RoundToInt(direction.z * maxPullDistance));

                // Ensure the pull position does not go beyond the caster
                pullPosition.x = Mathf.Clamp(pullPosition.x, this.pawn.Position.x + 1, this.pawn.Position.x - 1);
                pullPosition.y = Mathf.Clamp(pullPosition.y, this.pawn.Position.y + 1, this.pawn.Position.y - 1);
                pullPosition.z = Mathf.Clamp(pullPosition.z, this.pawn.Position.z + 1, this.pawn.Position.z - 1);

                if (target.Thing is Pawn)
                {
                    AbilityPawnFlyer flyer = (AbilityPawnFlyer)PawnFlyer.MakeFlyer(VFE_DefOf_Abilities.VFEA_AbilityFlyer, target.Thing as Pawn, pullPosition, null, null);
                    flyer.ability = this;
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


    }
}
