using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_Ability_SunDjerm : Lightsaber_AbilityBase
    {
        public override void AttackTarget(LocalTargetInfo target)
        {
            if (target.Pawn != null && target.Pawn.equipment != null && target.Pawn.equipment.Primary != null)
            {
                ThingWithComps weapon = target.Pawn.equipment.Primary;
                target.Pawn.equipment.TryDropEquipment(weapon, out ThingWithComps droppedWeapon, target.Pawn.Position, true);
                if (droppedWeapon != null)
                {
                    droppedWeapon.Destroy();
                }
            }
        }
    }
}