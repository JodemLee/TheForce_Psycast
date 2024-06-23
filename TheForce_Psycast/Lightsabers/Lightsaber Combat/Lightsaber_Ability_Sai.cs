using RimWorld;
using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_Ability_Sai : Lightsaber_AbilityBase
    {
        public override void AttackTarget(LocalTargetInfo target)
        {
            var map = Caster.Map;
            var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawn, CasterPawn, target.Cell, null, null);
            GenSpawn.Spawn(flyer, Caster.Position, map);
            this.pawn.Position = target.Cell;
            this.pawn.stances.SetStance(new Stance_Mobile());
            this.pawn.meleeVerbs.TryMeleeAttack(target.Pawn, null, true);
        }

    }
}