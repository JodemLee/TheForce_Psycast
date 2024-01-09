using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    public class Ability_Forcepush : VFECore.Abilities.Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
    {
        foreach (GlobalTargetInfo target in targets)
        {
            // Calculate the offset between caster and target
            IntVec3 offset = this.pawn.Position - targets[0].Cell;

            // Ensure the offset magnitude is at least 3 cells
            if (offset.LengthManhattan < 3)
            {
                offset = offset * 3;
            }

            // Calculate the new position by pushing back from the original position
            IntVec3 pushBackPosition = targets[0].Cell - offset;

            if (target.Thing is Pawn)
            {
                AbilityPawnFlyer flyer = (AbilityPawnFlyer)PawnFlyer.MakeFlyer(VFE_DefOf_Abilities.VFEA_AbilityFlyer, target.Thing as Pawn, pushBackPosition, null, null);
                flyer.ability = this;
                flyer.target = pushBackPosition.ToVector3();
                GenSpawn.Spawn(flyer, this.pawn.Position, this.pawn.Map);
            }
            else
            {
                target.Thing.Position = pushBackPosition;
            }

            base.Cast(targets);
        }
        }
    }
}



