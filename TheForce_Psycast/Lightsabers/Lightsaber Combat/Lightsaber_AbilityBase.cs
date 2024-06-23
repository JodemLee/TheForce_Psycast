using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_AbilityBase : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (targets.Length > 0)
            {
                AttackTarget((LocalTargetInfo)targets[0]);
            }
        }

        public virtual void AttackTarget(LocalTargetInfo target)
        {
            if (target.Pawn != null)
            {
                Log.Message($"Attacking target: {target.Pawn.Name}");
                pawn.meleeVerbs.TryMeleeAttack(target.Pawn, null, true);
            }
            else
            {
                Log.Message("Invalid target.");
            }
        }
    }
}
