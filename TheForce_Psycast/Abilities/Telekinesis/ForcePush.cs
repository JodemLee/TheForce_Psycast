using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheForce_Psycast
{
    public class Ability_Forcepush : VFECore.Abilities.Ability
    {
        // Add a variable to store the base damage amount
        public float baseDamage = 1f;
        Force_ModSettings modSettings = new Force_ModSettings();
        public bool usePsycastStat = false;
        public int offsetMultiplier { get; set; }

        public Ability_Forcepush()
        {
            modSettings = new Force_ModSettings(); // Instantiate Force_ModSettings
        }

        public int GetOffsetMultiplier()
        {

            if (Force_ModSettings.usePsycastStat == true) 
            {
                offsetMultiplier = (int)(offsetMultiplier * pawn.GetStatValue(StatDefOf.PsychicSensitivity));
                return offsetMultiplier;
            }
            else
            {
                offsetMultiplier = (int)Force_ModSettings.offSetMultiplier;
                
            }
            return offsetMultiplier;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            int offsetMultiplier = GetOffsetMultiplier();

            foreach (GlobalTargetInfo target in targets)
            {
                // Calculate the offset between caster and target
                IntVec3 offset = target.Cell - this.pawn.Position;

                // Ensure the offset magnitude is at least 3 cells
                if (offset.LengthHorizontal < 3)
                {
                    offset = offset * 3;
                }

                offset *= offsetMultiplier;

                // Calculate the new position by pushing back from the original position
                IntVec3 pushBackPosition = target.Cell + offset;

                // Check if the new position intersects with a solid object (e.g., a wall)
                if (!PositionUtils.CheckValidPosition(pushBackPosition, Caster.Map))
                {
                    // If it intersects, find a valid position closer to the target
                    pushBackPosition = PositionUtils.FindValidPosition(target.Cell, offset, Caster.Map);

                    float distancePushed = (pushBackPosition - target.Cell).LengthHorizontal;

                    // Scale the damage based on the distance pushed
                    float scaledDamage = baseDamage + (distancePushed * 2f); // Adjust the multiplier as needed

                    // Apply damage to the pawn when it hits a wall
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, (int)scaledDamage, 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                    target.Thing.TakeDamage(damageInfo);
                }

                if (target.Thing is Pawn pawn)
                {
                    var map = Caster.Map;
                    var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawn, pawn, pushBackPosition, null, null);
                    GenSpawn.Spawn(flyer, pushBackPosition, map);
                }
                else
                {
                    target.Thing.Position = pushBackPosition;
                }
            }

            base.Cast(targets);
        }
    }
}



