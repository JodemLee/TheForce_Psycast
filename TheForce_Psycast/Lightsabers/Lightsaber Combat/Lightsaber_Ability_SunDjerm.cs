using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal class Lightsaber_Ability_SunDjerm : Lightsaber_AbilityBase
    {
        public override void AttackTarget(LocalTargetInfo target)
        {
            base.AttackTarget(target);

            // Check if the target has a weapon
            if (target.Pawn != null && target.Pawn.equipment != null && target.Pawn.equipment.Primary != null)
            {
                // Drop the weapon from the pawn's hand
                ThingWithComps weapon = target.Pawn.equipment.Primary;
                target.Pawn.equipment.TryDropEquipment(weapon, out ThingWithComps droppedWeapon, target.Pawn.Position, true);

                // Destroy the dropped weapon
                if (droppedWeapon != null)
                {
                    droppedWeapon.Destroy();
                }
            }
        }
    }
}