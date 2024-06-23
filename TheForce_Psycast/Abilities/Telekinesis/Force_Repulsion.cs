using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    public class Ability_ForceRepulsion : VFECore.Abilities.Ability
    {
        Force_ModSettings modSettings = new Force_ModSettings();
        public bool usePsycastStat = false;
        public int offsetMultiplier { get; set; }

        public Ability_ForceRepulsion()
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

                // Normalize the offset to ensure correct direction
                IntVec3 normalizedOffset = NormalizeIntVec3(offset);

                // Apply the offset multiplier
                normalizedOffset *= offsetMultiplier;

                // Calculate the new position by pushing back from the original position
                IntVec3 pushBackPosition = target.Cell + normalizedOffset;

                // Check if the new position intersects with a solid object (e.g., a wall)
                if (!IsPositionValid(pushBackPosition))
                {
                    // If it intersects, find a valid position closer to the target
                    pushBackPosition = FindValidPosition(target.Cell, normalizedOffset);
                }

                // Apply explosive force to the target
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

        public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 2));

        private IntVec3 NormalizeIntVec3(IntVec3 vector)
        {
            float magnitude = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);

            if (magnitude > 0)
            {
                float scaleFactor = 1f / magnitude;
                return new IntVec3(Mathf.RoundToInt(vector.x * scaleFactor), 0, Mathf.RoundToInt(vector.z * scaleFactor));
            }

            return vector;
        }

        // Helper method to check if the position is valid
        private bool IsPositionValid(IntVec3 position)
        {
            return position.InBounds(this.pawn.Map) && position.Walkable(this.pawn.Map) && position.Standable(this.pawn.Map);
        }

        // Helper method to find a valid position closer to the target
        private IntVec3 FindValidPosition(IntVec3 originalPosition, IntVec3 offset)
        {
            Map map = this.pawn.Map;
            IntVec3 closestValidPosition = originalPosition;
            float closestDistanceSquared = float.MaxValue;

            for (int i = 1; i <= 3; i++)
            {
                IntVec3 newPosition = originalPosition + (offset * i);
                if (IsPositionValid(newPosition))
                {
                    // If the new position is valid, calculate its distance to the original position
                    float distanceSquared = newPosition.DistanceToSquared(originalPosition);
                    if (distanceSquared < closestDistanceSquared)
                    {
                        // Update the closest valid position if this position is closer
                        closestValidPosition = newPosition;
                        closestDistanceSquared = distanceSquared;
                    }
                }
            }
            // Return the closest valid position found
            return closestValidPosition;
        }
    }
}
